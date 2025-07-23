using System.ComponentModel.DataAnnotations;

namespace TaskService.Controllers.DTO.Requests;

public class CreateJobRequest
{
    [Required] 
    [MaxLength(100)] 
    public string Name { get; set; } = null!;
    
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = null!;
    
    public DateTime? Deadline { get; set; }
}