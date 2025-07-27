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

            Create.Index("idx_notifications_user_id").OnTable("notifications").OnColumn("user_id").Ascending();
            Create.Index("idx_notifications_user_id_is_read").OnTable("notifications")
                .OnColumn("user_id").Ascending()
                .OnColumn("is_read").Ascending();
            Create.Index("idx_notifications_created_at").OnTable("notifications").OnColumn("created_at").Descending();
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