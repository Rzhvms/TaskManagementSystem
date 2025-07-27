using Dapper.FluentMap.Mapping;
using AuthService.Data.Entities;

namespace AuthService.Data.Mappings;

public class UserMap : EntityMap<User>
{
    public UserMap()
    {
        Map(u => u.Id).ToColumn("id");
        Map(u => u.Username).ToColumn("username");
        Map(u => u.Email).ToColumn("email");
        Map(u => u.PasswordHash).ToColumn("password_hash");
        Map(u => u.CreatedAt).ToColumn("created_at");
    }
}