using TaskService.Controllers.DTO.Requests;

namespace TaskService.Logic.Services.Interfaces;

public interface INotificationClient
{
    Task SendNotificationAsync(CreateNotificationRequest request);
}