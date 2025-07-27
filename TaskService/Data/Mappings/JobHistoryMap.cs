using Dapper.FluentMap.Mapping;
using TaskService.Data.Entities;

namespace TaskService.Data.Mappings;

public class JobHistoryMap : EntityMap<JobHistory>
{
    public JobHistoryMap()
    {
        Map(j => j.Id).ToColumn("id");
        Map(j => j.JobId).ToColumn("job_id");
        Map(j => j.ChangedBy).ToColumn("changed_by");
        Map(j => j.ChangedAt).ToColumn("changed_at");
        Map(j => j.Action).ToColumn("action");
        Map(j => j.OldValue).ToColumn("old_value");
        Map(j => j.NewValue).ToColumn("new_value");
    }
}