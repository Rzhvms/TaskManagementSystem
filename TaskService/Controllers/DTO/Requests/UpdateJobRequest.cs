using System.ComponentModel.DataAnnotations;
using TaskService.Data.Entities;

namespace TaskService.Controllers.DTO.Requests;

public class UpdateJobRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime? Deadline { get; set; }

    [Required]
    public JobStatus Status { get; set; }
}