using Dapper.FluentMap.Mapping;
using TaskService.Data.Entities;

namespace TaskService.Data.Mappings;

public class JobMap : EntityMap<Job>
{
    public JobMap()
    {
        Map(j => j.Id).ToColumn("id");
        Map(j => j.Name).ToColumn("name");
        Map(j => j.Description).ToColumn("description");
        Map(j => j.Status).ToColumn("status");
        Map(j => j.CreatedAt).ToColumn("created_at");
        Map(j => j.UpdatedAt).ToColumn("updated_at");
        Map(j => j.Deadline).ToColumn("deadline");
        Map(j => j.CreatedBy).ToColumn("created_by");
        Map(j => j.PerformerId).ToColumn("performer_id");
        Map(j => j.IsDeleted).ToColumn("is_deleted");
    }
}