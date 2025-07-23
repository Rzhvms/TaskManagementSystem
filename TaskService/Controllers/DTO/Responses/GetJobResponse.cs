using TaskService.Data.Entities;

namespace TaskService.Controllers.DTO.Responses;

public class GetJobResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public JobStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? Deadline { get; set; }
    public int CreatedBy { get; set; }
    public int? PerformerId { get; set; }
}