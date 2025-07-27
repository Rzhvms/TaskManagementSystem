using NotificationService.Controllers.DTO.Requests;
using NotificationService.Controllers.DTO.Responses;
using NotificationService.Data.Entities;
using NotificationService.Data.Repositories.Interfaces;
using NotificationService.Logic.Services.Interfaces;
using NotificationService.Logic.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Logic.Services;

public class NotificationsService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly ILogger<NotificationsService> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationsService(
        INotificationRepository repository, 
        ILogger<NotificationsService> logger,
        IHubContext<NotificationHub> hubContext
        )
    {
        _repository = repository;
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task<IEnumerable<GetNotificationResponse>> GetNotificationsByUserIdAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Получение уведомлений для пользователя с ID {UserId}", userId);
            var notifications = await _repository.GetByUserIdAsync(userId);
            return notifications.Select(MapToResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении уведомлений для пользователя с ID {UserId}", userId);
            throw;
        }
    }

    public async Task<int> CreateNotificationAsync(CreateNotificationRequest request)
    {
        try
        {
            _logger.LogInformation("Создание уведомления: UserId={UserId}, Title='{Title}'", request.UserId, request.Title);

            var createdAt = DateTime.UtcNow;

            var notification = new Notification
            {
                UserId = request.UserId,
                Title = request.Title,
                Message = request.Message,
                IsRead = false,
                CreatedAt = createdAt
            };

            var notificationId = await _repository.CreateAsync(notification);

            await SendSignalRNotificationAsync(request.UserId, notificationId, request.Title, request.Message, createdAt);

            _logger.LogInformation("Уведомление успешно создано и отправлено через SignalR: ID={NotificationId}", notificationId);

            return notificationId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании уведомления для пользователя с ID {UserId}", request.UserId);
            throw;
        }
    }

    public async Task<bool> MarkAsReadAsync(int notificationId)
    {
        try
        {
            _logger.LogInformation("Пометка уведомления как прочитанного. ID = {NotificationId}", notificationId);

            var result = await _repository.MarkAsReadAsync(notificationId);

            if (result)
                _logger.LogInformation("Уведомление с ID {NotificationId} успешно помечено как прочитанное", notificationId);
            else
                _logger.LogWarning("Уведомление с ID {NotificationId} не найдено для пометки как прочитанное", notificationId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при пометке уведомления как прочитанного. ID = {NotificationId}", notificationId);
            throw;
        }
    }
    
    private async Task SendSignalRNotificationAsync(int userId, int notificationId, string title, string message, DateTime createdAt)
    {
        await _hubContext.Clients
            .User(userId.ToString())
            .SendAsync("ReceiveNotification", new
            {
                Id = notificationId,
                Title = title,
                Message = message,
                CreatedAt = createdAt
            });
    }
    
    private GetNotificationResponse MapToResponse(Notification notification)
    {
        return new GetNotificationResponse
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Title = notification.Title,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
    }
}