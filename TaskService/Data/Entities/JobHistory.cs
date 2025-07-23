namespace TaskService.Data.Entities;

public class JobHistory
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public int ChangedBy { get; set; }
    public JobActionType Action { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}