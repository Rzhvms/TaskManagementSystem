using System.Data;
using Dapper;
using NotificationService.Data.Entities;
using NotificationService.Data.Repositories.Interfaces;

namespace NotificationService.Data.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly IDbConnection _connection;

    public NotificationRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId)
    {
        var sql = "SELECT * FROM notifications WHERE user_id = @UserId ORDER BY created_at DESC";
        return await _connection.QueryAsync<Notification>(sql, new { UserId = userId });
    }

    public async Task<int> CreateAsync(Notification notification)
    {
        var sql = @"INSERT INTO notifications (user_id, title, message, is_read, created_at)
                    VALUES (@UserId, @Title, @Message, @IsRead, @CreatedAt)
                    RETURNING id";
        return await _connection.ExecuteScalarAsync<int>(sql, notification);
    }

    public async Task<bool> MarkAsReadAsync(int id)
    {
        var sql = "UPDATE notifications SET is_read = TRUE WHERE id = @Id";
        var result = await _connection.ExecuteAsync(sql, new { Id = id });
        return result > 0;
    }
}