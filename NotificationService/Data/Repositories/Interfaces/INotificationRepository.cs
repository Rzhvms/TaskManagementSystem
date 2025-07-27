using NotificationService.Data.Entities;

namespace NotificationService.Data.Repositories.Interfaces;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);
    Task<int> CreateAsync(Notification notification);
    Task<bool> MarkAsReadAsync(int id);
}