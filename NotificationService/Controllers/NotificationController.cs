using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Controllers.DTO.Requests;
using NotificationService.Logic.Services.Interfaces;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationController(INotificationService service)
    {
        _service = service;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        var notifications  = await _service.GetNotificationsByUserIdAsync(userId);
        return Ok(notifications);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNotificationRequest request)
    {
        var notificationId = await _service.CreateNotificationAsync(request);
        return CreatedAtAction(
            nameof(GetByUserId), 
            new { userId = request.UserId }, 
            new { notificationId });
    }

    [HttpPut("{id}/mark-as-read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var updated = await _service.MarkAsReadAsync(id);
        return updated ? Ok("Уведомление прочитано.") : NotFound();
    }
}