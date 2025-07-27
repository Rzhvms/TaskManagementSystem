using TaskService.Data.Entities;

namespace TaskService.Data.Repositories.Interfaces;

public interface IJobHistoryRepository
{
    Task AddAsync(JobHistory history);
}