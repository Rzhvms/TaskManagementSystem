using FluentMigrator;

namespace AuthService.Data.Migrations;

[Migration(202507240002)]
public class CreateUserTable : Migration
{
    public override void Up()
    {
        Create.Table("users")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("username").AsString(100).NotNullable()
            .WithColumn("email").AsString(255).NotNullable().Unique()
            .WithColumn("password_hash").AsString(255).NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

        Create.Index("idx_users_email").OnTable("users").OnColumn("email").Ascending();
        Create.Index("idx_users_username").OnTable("users").OnColumn("username").Ascending();
    }

    public override void Down()
    {
        Delete.Table("users");
    }
}