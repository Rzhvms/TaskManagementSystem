using System.ComponentModel.DataAnnotations;

namespace TaskService.Controllers.DTO.Requests;

public class AssignPerformerRequest
{
    [Required]
    public int PerformerId { get; set; }
}