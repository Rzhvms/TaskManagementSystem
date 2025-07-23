using TaskService.Data.Entities;
using TaskService.Data.Interfaces;
using TaskService.Controllers.DTO.Requests;
using TaskService.Controllers.DTO.Responses;

namespace TaskService.Logic.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _repository;

    public JobService(IJobRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> CreateJobAsync(CreateJobRequest request, int userId)
    {
        var job = new Job
        {
            Name = request.Name,
            Description = request.Description,
            Deadline = request.Deadline,
            CreatedBy = userId,
            Status = JobStatus.ToDo,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        return await _repository.CreateAsync(job);
    }

    public async Task<GetJobResponse?> GetJobByIdAsync(int id)
    {
        var job = await _repository.GetByIdAsync(id);
        if (job == null || job.IsDeleted) return null;

        return MapToResponse(job);
    }

    public async Task<IEnumerable<GetJobResponse>> GetFilteredJobsAsync(JobFilterRequest filter)
    {
        var jobs = await _repository.GetFilteredAsync(filter);
        
        return jobs
            .Where(j => !j.IsDeleted)
            .Select(MapToResponse);
    }

    public async Task<bool> UpdateJobAsync(int id, UpdateJobRequest request, int userId)
    {
        var job = await _repository.GetByIdAsync(id);
        if (job == null || job.IsDeleted) return false;

        job.Name = request.Name;
        job.Description = request.Description ?? string.Empty;
        job.Deadline = request.Deadline;
        job.Status = request.Status;
        job.UpdatedAt = DateTime.UtcNow;

        return await _repository.UpdateAsync(job);
    }

    public async Task<bool> DeleteJobAsync(int id, int userId)
    {
        var job = await _repository.GetByIdAsync(id);
        if (job == null || job.IsDeleted) return false;

        return await _repository.DeleteAsync(id);
    }

    public async Task<bool> AssignPerformerAsync(int jobId, int performerId, int userId)
    {
        var job = await _repository.GetByIdAsync(jobId);
        if (job == null || job.IsDeleted) return false;

        job.PerformerId = performerId;
        job.UpdatedAt = DateTime.UtcNow;

        return await _repository.AssignPerformerAsync(jobId, performerId);
    }

    private GetJobResponse MapToResponse(Job job)
    {
        return new GetJobResponse
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
}
