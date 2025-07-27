using Dapper.FluentMap.Mapping;
using NotificationService.Data.Entities;

namespace NotificationService.Data.Mappings;

public class NotificationMap : EntityMap<Notification>
{
    public NotificationMap()
    {
        Map(n => n.Id).ToColumn("id");
        Map(n => n.UserId).ToColumn("user_id");
        Map(n => n.Title).ToColumn("title");
        Map(n => n.Message).ToColumn("message");
        Map(n => n.IsRead).ToColumn("is_read");
        Map(n => n.CreatedAt).ToColumn("created_at");
    }
}