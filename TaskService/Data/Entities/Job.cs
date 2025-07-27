namespace TaskService.Data.Entities;

public class Job
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public JobStatus Status { get; set; } = JobStatus.ToDo;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? Deadline { get; set; }
    public int CreatedBy { get; set; }
    public int? PerformerId { get; set; }
    public bool IsDeleted { get; set; } = false;
}