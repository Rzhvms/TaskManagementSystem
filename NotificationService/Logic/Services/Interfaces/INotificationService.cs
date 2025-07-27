using NotificationService.Controllers.DTO.Requests;
using NotificationService.Controllers.DTO.Responses;

namespace NotificationService.Logic.Services.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<GetNotificationResponse>> GetNotificationsByUserIdAsync(int userId);
    Task<int> CreateNotificationAsync(CreateNotificationRequest request);
    Task<bool> MarkAsReadAsync(int notificationId);
}