using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Logic.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public async Task SendNotification(int userId, string message)
    {
        await Clients.User(userId.ToString()).SendAsync("ReceiveNotification", message);
    }
}