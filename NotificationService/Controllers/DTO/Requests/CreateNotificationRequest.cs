using System.ComponentModel.DataAnnotations;

namespace NotificationService.Controllers.DTO.Requests;

public class CreateNotificationRequest
{
    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = null!;

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = null!;
}