using TaskService.Data.Entities;

namespace TaskService.Controllers.DTO.Requests;

public class JobFilterRequest
{
    public JobStatus? Status { get; set; }
    public int? PerformerId { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? DeadlineFrom { get; set; }
    public DateTime? DeadlineTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}