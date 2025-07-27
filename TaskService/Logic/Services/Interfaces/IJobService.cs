using TaskService.Controllers.DTO.Requests;
using TaskService.Controllers.DTO.Responses;

namespace TaskService.Logic.Services.Interfaces;

public interface IJobService
{
    Task<int> CreateJobAsync(CreateJobRequest request, int userId);
    Task<GetJobResponse?> GetJobByIdAsync(int id);
    Task<IEnumerable<GetJobResponse>> GetFilteredJobsAsync(JobFilterRequest filter);
    Task<bool> UpdateJobAsync(int id, UpdateJobRequest request, int userId);
    Task<bool> DeleteJobAsync(int id, int userId);
    Task<bool> AssignPerformerAsync(int jobId, int performerId, int userId);
}