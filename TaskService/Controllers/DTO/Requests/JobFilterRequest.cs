using System.ComponentModel.DataAnnotations;
using TaskService.Data.Entities;

namespace TaskService.Controllers.DTO.Requests;

public class JobFilterRequest
{
    public JobStatus? Status { get; set; }
    public int? PerformerId { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? DeadlineFrom { get; set; }
    public DateTime? DeadlineTo { get; set; }
    
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
    
    [Range(1, 100)]
    public int PageSize { get; set; } = 20;
}