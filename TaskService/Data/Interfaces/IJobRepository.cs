using TaskService.Controllers.DTO.Requests;
using TaskService.Data.Entities;

namespace TaskService.Data.Interfaces;

public interface IJobRepository
{
    Task<Job?> GetByIdAsync(int id);
    Task<IEnumerable<Job>> GetFilteredAsync(JobFilterRequest filter);
    Task<int> CreateAsync(Job job);
    Task<bool> UpdateAsync(Job job);
    Task<bool> DeleteAsync(int id);
    Task<bool> AssignPerformerAsync(int jobId, int performerId);
}