using FluentMigrator;

namespace TaskService.Data.Migrations;

[Migration(202507230001)]
public class CreateJobsAndJobHistoriesTables : Migration
{
    public override void Up()
    {
        if (!Schema.Table("jobs").Exists())
        {
            Create.Table("jobs")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("name").AsString(100).NotNullable()
                .WithColumn("description").AsString(1000).Nullable()
                .WithColumn("status").AsString(20).NotNullable().WithDefaultValue("ToDo")
                .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("updated_at").AsDateTime().Nullable()
                .WithColumn("deadline").AsDateTime().Nullable()
                .WithColumn("created_by").AsInt32().NotNullable()
                .WithColumn("performer_id").AsInt32().Nullable()
                .WithColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false);

            Create.Index("idx_jobs_status").OnTable("jobs").OnColumn("status").Ascending();
            Create.Index("idx_jobs_performer_id").OnTable("jobs").OnColumn("performer_id").Ascending();
            Create.Index("idx_jobs_created_by").OnTable("jobs").OnColumn("created_by").Ascending();
            Create.Index("idx_jobs_deadline").OnTable("jobs").OnColumn("deadline").Ascending();
        }

        if (!Schema.Table("job_histories").Exists())
        {
            Create.Table("job_histories")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("job_id").AsInt32().NotNullable()
                .WithColumn("changed_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("changed_by").AsInt32().NotNullable()
                .WithColumn("action").AsString(50).NotNullable()
                .WithColumn("old_value").AsString(int.MaxValue).Nullable()
                .WithColumn("new_value").AsString(int.MaxValue).Nullable();

            Create.ForeignKey("fk_job_histories_jobs")
                .FromTable("job_histories").ForeignColumn("job_id")
                .ToTable("jobs").PrimaryColumn("id")
                .OnDeleteOrUpdate(System.Data.Rule.Cascade);

            Create.Index("idx_job_histories_job_id").OnTable("job_histories").OnColumn("job_id").Ascending();
            Create.Index("idx_job_histories_changed_at").OnTable("job_histories").OnColumn("changed_at").Ascending();
        }
    }

    public override void Down()
    {
        if (Schema.Table("job_histories").Exists())
        {
            Delete.Table("job_histories");
        }

        if (Schema.Table("jobs").Exists())
        {
            Delete.Table("jobs");
        }
    }
}