using System.Data;
using Dapper;
using TaskService.Data.Entities;
using TaskService.Data.Repositories.Interfaces;

namespace TaskService.Data.Repositories;

public class JobHistoryRepository : IJobHistoryRepository
{
    private readonly IDbConnection _db;

    public JobHistoryRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task AddAsync(JobHistory history)
    {
        var query = @"
            INSERT INTO job_histories (job_id, changed_by, changed_at, action, old_value, new_value)
            VALUES (@JobId, @ChangedBy, @ChangedAt, @Action, @OldValue, @NewValue)";

        await _db.ExecuteAsync(query, history);
    }
}