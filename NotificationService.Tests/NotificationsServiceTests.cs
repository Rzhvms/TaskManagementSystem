using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Controllers.DTO.Requests;
using NotificationService.Data.Entities;
using NotificationService.Data.Repositories.Interfaces;
using NotificationService.Logic.Hubs;
using NotificationService.Logic.Services;

namespace NotificationService.Tests;

public class NotificationsServiceTests
{
    private readonly Mock<INotificationRepository> _repositoryMock = new();
    private readonly Mock<ILogger<NotificationsService>> _loggerMock = new();
    private readonly Mock<IHubContext<NotificationHub>> _hubContextMock = new();
    private readonly NotificationsService _sut;

    public NotificationsServiceTests()
    {
        _sut = new NotificationsService(
            _repositoryMock.Object,
            _loggerMock.Object,
            _hubContextMock.Object
        );
    }

    [Fact]
    public async Task GetNotificationsByUserIdAsync_ShouldReturnMappedNotifications()
    {
        const int userId = 1;
        var testNotifications = new List<Notification>
        {
            CreateNotification(1, userId),
            CreateNotification(2, userId)
        };

        _repositoryMock
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(testNotifications);

        var result = await _sut.GetNotificationsByUserIdAsync(userId);

        result.Should().HaveCount(2);
        result.First().Id.Should().Be(1);

        VerifyLog(LogLevel.Information, $"Получение уведомлений для пользователя с ID {userId}");
    }

    [Fact]
    public async Task GetNotificationsByUserIdAsync_WhenRepositoryThrows_ShouldLogAndThrow()
    {
        const int userId = 99;
        var exception = new Exception("DB error");

        _repositoryMock
            .Setup(r => r.GetByUserIdAsync(userId))
            .ThrowsAsync(exception);

        var act = async () => await _sut.GetNotificationsByUserIdAsync(userId);

        await act.Should().ThrowAsync<Exception>();
        VerifyLog(LogLevel.Error, $"Ошибка при получении уведомлений для пользователя с ID {userId}", exception);
    }

    [Fact]
    public async Task CreateNotificationAsync_ShouldCreateAndSendSignalRNotification()
    {
        var request = new CreateNotificationRequest
        {
            UserId = 1,
            Title = "New Task",
            Message = "You have a new task"
        };
        const int createdId = 123;
        var clientProxyMock = new Mock<IClientProxy>();

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Notification>()))
            .ReturnsAsync(createdId);

        _hubContextMock
            .Setup(h => h.Clients.User(request.UserId.ToString()))
            .Returns(clientProxyMock.Object);

        var result = await _sut.CreateNotificationAsync(request);

        result.Should().Be(createdId);

        _repositoryMock.Verify(r => r.CreateAsync(It.Is<Notification>(n =>
            n.UserId == request.UserId && n.Title == request.Title && n.Message == request.Message)), Times.Once);

        clientProxyMock.Verify(c => c.SendCoreAsync(
            "ReceiveNotification",
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()), Times.Once);

        VerifyLog(LogLevel.Information, $"Уведомление успешно создано и отправлено через SignalR: ID={createdId}");
    }

    [Fact]
    public async Task MarkAsReadAsync_WhenExists_ShouldReturnTrue()
    {
        const int notificationId = 1;

        _repositoryMock
            .Setup(r => r.MarkAsReadAsync(notificationId))
            .ReturnsAsync(true);

        var result = await _sut.MarkAsReadAsync(notificationId);

        result.Should().BeTrue();
        VerifyLog(LogLevel.Information, $"Уведомление с ID {notificationId} успешно помечено как прочитанное");
    }

    [Fact]
    public async Task MarkAsReadAsync_WhenNotExists_ShouldReturnFalse()
    {
        const int notificationId = 999;

        _repositoryMock
            .Setup(r => r.MarkAsReadAsync(notificationId))
            .ReturnsAsync(false);

        var result = await _sut.MarkAsReadAsync(notificationId);

        result.Should().BeFalse();
        VerifyLog(LogLevel.Warning, $"Уведомление с ID {notificationId} не найдено");
    }
    
    private static Notification CreateNotification(int id, int userId, bool isRead = false) => new()
    {
        Id = id,
        UserId = userId,
        Title = "Test Title",
        Message = "Test Message",
        IsRead = isRead,
        CreatedAt = DateTime.UtcNow
    };

    private void VerifyLog(LogLevel level, string expectedMessage, Exception? exception = null)
    {
        _loggerMock.Verify(logger => logger.Log(
            level,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((obj, _) => obj.ToString()!.Contains(expectedMessage)),
            exception ?? It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}