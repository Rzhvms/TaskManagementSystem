using TaskService.Data.Entities;
using TaskService.Data.Repositories.Interfaces;
using TaskService.Controllers.DTO.Requests;
using TaskService.Controllers.DTO.Responses;
using TaskService.Logic.Services.Interfaces;
using System.Text.Json;

namespace TaskService.Logic.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _repository;
    private readonly IJobHistoryRepository _historyRepository;
    private readonly ILogger<JobService> _logger;
    private readonly INotificationClient _notificationClient;

    public JobService(
        IJobRepository repository,
        IJobHistoryRepository historyRepository,
        ILogger<JobService> logger,
        INotificationClient notificationClient)
    {
        _repository = repository;
        _historyRepository = historyRepository;
        _logger = logger;
        _notificationClient = notificationClient;
    }

    public async Task<int> CreateJobAsync(CreateJobRequest request, int userId)
    {
        try
        {
            var job = new Job
            {
                Name = request.Name,
                Description = request.Description,
                Deadline = request.Deadline,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                Status = JobStatus.ToDo,
                IsDeleted = false
            };

            var jobId = await _repository.CreateAsync(job);

            await LogHistoryAsync(jobId, userId, DateTime.UtcNow, JobActionType.Created, null, Serialize(job));

            _logger.LogInformation("Задача создана. JobId: {JobId}, Пользователь: {UserId}", jobId, userId);

            return jobId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании задачи. Пользователь: {UserId}", userId);
            throw;
        }
    }

    public async Task<GetJobResponse?> GetJobByIdAsync(int id)
    {
        try
        {
            var job = await GetValidJobAsync(id);
            if (job == null) 
                return null;

            return Map(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении задачи. JobId: {JobId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<GetJobResponse>> GetFilteredJobsAsync(JobFilterRequest filter)
    {
        try
        {
            var jobs = await _repository.GetFilteredAsync(filter);
            _logger.LogInformation("Получены задачи по фильтру: {@Filter}", filter);
            return jobs.Select(Map);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении задач по фильтру: {@Filter}", filter);
            throw;
        }
    }

    public async Task<bool> UpdateJobAsync(int id, UpdateJobRequest request, int userId)
    {
        try
        {
            var job = await GetValidJobAsync(id);
            if (job == null) 
                return false;

            if (job.CreatedBy != userId)
            {
                _logger.LogWarning("Обновление запрещено. Пользователь {UserId} не является создателем. JobId: {JobId}", userId, id);
                return false;
            }

            var oldValue = Serialize(job);

            job.Name = request.Name;
            job.Description = request.Description ?? string.Empty;
            job.Deadline = request.Deadline;
            job.Status = request.Status;
            job.UpdatedAt = DateTime.UtcNow;

            var updated = await _repository.UpdateAsync(job);
            if (!updated)
            {
                _logger.LogWarning("Обновление не удалось в репозитории. JobId: {JobId}", id);
                return false;
            }

            var newValue = Serialize(job);
            
            if (oldValue != newValue)
                await LogHistoryAsync(id, userId, DateTime.UtcNow, JobActionType.Updated, oldValue, newValue);
            else
                _logger.LogInformation("Обновление не внесло изменений. JobId: {JobId}, Пользователь: {UserId}", id, userId);

            _logger.LogInformation("Задача обновлена. JobId: {JobId}, Пользователь: {UserId}", id, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении задачи. JobId: {JobId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteJobAsync(int id, int userId)
    {
        try
        {
            var job = await GetValidJobAsync(id);
            if (job == null) 
                return false;

            var oldValue = Serialize(job);
            var result = await _repository.DeleteAsync(id);
            if (!result)
            {
                _logger.LogWarning("Ошибка удаления в репозитории. JobId: {JobId}", id);
                return false;
            }

            await LogHistoryAsync(id, userId, DateTime.UtcNow, JobActionType.Deleted, oldValue, null);
            
            _logger.LogInformation("Задача удалена. JobId: {JobId}, Пользователь: {UserId}", id, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении задачи. JobId: {JobId}", id);
            throw;
        }
    }

    public async Task<bool> AssignPerformerAsync(int jobId, int performerId, int userId)
    {
        try
        {
            var job = await GetValidJobAsync(jobId);
            if (job == null) 
                return false;

            var oldValue = Serialize(job);

            job.PerformerId = performerId;
            job.UpdatedAt = DateTime.UtcNow;

            var result = await _repository.AssignPerformerAsync(jobId, performerId);
            if (!result)
            {
                _logger.LogWarning("Ошибка назначения исполнителя в репозитории. JobId: {JobId}", jobId);
                return false;
            }

            var newValue = Serialize(job);

            await LogHistoryAsync(jobId, userId, DateTime.UtcNow, JobActionType.Assigned, oldValue, newValue);
            
            _logger.LogInformation("Исполнитель назначен. JobId: {JobId}, PerformerId: {PerformerId}, Пользователь: {UserId}",
                jobId, performerId, userId);
            
            await _notificationClient.SendNotificationAsync(new CreateNotificationRequest
            {
                UserId = performerId,
                Title = "Вам назначена новая задача",
                Message = $"Задача: {job.Name}, дедлайн: {job.Deadline?.ToShortDateString() ?? "не указан"}"
            });
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при назначении исполнителя. JobId: {JobId}", jobId);
            throw;
        }
    }

    private async Task<Job?> GetValidJobAsync(int id)
    {
        var job = await _repository.GetByIdAsync(id);
        if (job == null || job.IsDeleted)
        {
            _logger.LogWarning("Задача не найдена или удалена. JobId: {JobId}", id);
            return null;
        }
        return job;
    }
    
    private async Task LogHistoryAsync(int jobId, int userId, DateTime changedAt, JobActionType action, string? oldValue, string? newValue)
    {
        await _historyRepository.AddAsync(new JobHistory
        {
            JobId = jobId,
            ChangedBy = userId,
            ChangedAt = changedAt,
            Action = action,
            OldValue = oldValue,
            NewValue = newValue
        });
    }
    
    private static string Serialize(Job job) => JsonSerializer.Serialize(job);

    private static GetJobResponse Map(Job job) =>
        new()
        {
            Id = job.Id,
            Name = job.Name,
            Description = job.Description,
            Status = job.Status,
            CreatedAt = job.CreatedAt,
            UpdatedAt = job.UpdatedAt,
            Deadline = job.Deadline,
            CreatedBy = job.CreatedBy,
            PerformerId = job.PerformerId
        };
}