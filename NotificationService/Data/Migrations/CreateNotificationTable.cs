using FluentMigrator;

namespace NotificationService.Data.Migrations;

[Migration(202507240001)]
public class CreateNotificationTable : Migration
{
    public override void Up()
    {
        if (!Schema.Table("notifications").Exists())
        {
            Create.Table("notifications")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("user_id").AsInt32().NotNullable()
                .WithColumn("title").AsString(200).NotNullable()
                .WithColumn("message").AsString(1000).NotNullable()
                .WithColumn("created_at").AsDateTime().NotNullable()
                .WithColumn("is_read").AsBoolean().NotNullable().WithDefaultValue(false);
        }
    }

    public override void Down()
    {
        if (Schema.Table("notifications").Exists())
        {
            Delete.Table("notifications");
        }
    }
}