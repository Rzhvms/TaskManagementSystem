using System.Data;
using Dapper;
using TaskService.Controllers.DTO.Requests;
using TaskService.Data.Entities;
using TaskService.Data.Repositories.Interfaces;

namespace TaskService.Data.Repositories;

public class JobRepository : IJobRepository
{
    private readonly IDbConnection _db;

    public JobRepository(IDbConnection db)
    {
        _db = db;
    }
    
    public async Task<Job?> GetByIdAsync(int id)
    {
        var query = @"
        SELECT 
            id,
            name,
            description,
            status,
            created_at,
            updated_at,
            deadline,
            created_by,
            performer_id,
            is_deleted
        FROM jobs 
        WHERE id = @Id AND is_deleted = false";

        return await _db.QueryFirstOrDefaultAsync<Job>(query, new { Id = id });
    }
    
    public async Task<IEnumerable<Job>> GetFilteredAsync(JobFilterRequest filter)
    {
        return await GetFilteredAsync(
            filter.Status,
            filter.PerformerId,
            filter.CreatedBy,
            filter.DeadlineFrom,
            filter.DeadlineTo,
            filter.Page,
            filter.PageSize);
    }
    
    private async Task<IEnumerable<Job>> GetFilteredAsync(JobStatus? status, int? performerId, 
        int? createdBy, DateTime? deadlineFrom, DateTime? deadlineTo, int page, int pageSize)
    {
        var query = @"
        SELECT 
            id,
            name,
            description,
            status,
            created_at,
            updated_at,
            deadline,
            created_by,
            performer_id,
            is_deleted
        FROM jobs
        WHERE is_deleted = false " + 
            (status.HasValue ? "AND status = @Status " : "") +
            (performerId.HasValue ? "AND performer_id = @PerformerId " : "") + 
            (createdBy.HasValue ? "AND created_by = @CreatedBy " : "") + 
            (deadlineFrom.HasValue ? "AND deadline >= @DeadlineFrom " : "") + 
            (deadlineTo.HasValue ? "AND deadline <= @DeadlineTo " : "") + 
            @"ORDER BY created_at DESC
            OFFSET @Offset LIMIT @PageSize";

        return await _db.QueryAsync<Job>(query, new
        {
            Status = status?.ToString(),
            PerformerId = performerId,
            CreatedBy = createdBy,
            DeadlineFrom = deadlineFrom,
            DeadlineTo = deadlineTo,
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        });
    }

    public async Task<int> CreateAsync(Job job)
    {
        var query = @"
            INSERT INTO jobs (name, description, status, created_at, deadline, created_by, performer_id, is_deleted)
            VALUES (@Name, @Description, @Status, @CreatedAt, @Deadline, @CreatedBy, @PerformerId, false)
            RETURNING id";
        
        return await _db.ExecuteScalarAsync<int>(query, new
        {
            job.Name,
            job.Description,
            Status = job.Status.ToString(),
            job.CreatedAt,
            job.Deadline,
            job.CreatedBy,
            job.PerformerId
        });
    }

    public async Task<bool> UpdateAsync(Job job)
    {
        var query = @"
            UPDATE jobs 
            SET
                name = @Name,
                description = @Description,
                status = @Status,
                deadline = @Deadline,
                updated_at = @UpdatedAt
            WHERE id = @Id AND is_deleted = false";

        var affected = await _db.ExecuteAsync(query, new
        {
            job.Name,
            job.Description,
            Status = job.Status.ToString(),
            job.Deadline,
            UpdatedAt = DateTime.UtcNow,
            job.Id
        });

        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var query = @"
            UPDATE jobs 
            SET is_deleted = true 
            WHERE id = @Id";
        
        return await _db.ExecuteAsync(query, new { Id = id }) > 0;
    }

    public async Task<bool> AssignPerformerAsync(int jobId, int performerId)
    {
        var query = @"
            UPDATE jobs 
            SET performer_id = @PerformerId, updated_at = @UpdatedAt 
            WHERE id = @JobId";
        
        return await _db.ExecuteAsync(query, new
        {
            PerformerId = performerId,
            JobId = jobId,
            UpdatedAt = DateTime.UtcNow
        }) > 0;
    }
}