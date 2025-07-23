namespace TaskService.Data.Entities;

public enum JobActionType
{
    Created,
    NameUpdated,
    DescriptionUpdated,
    DeadlineUpdated,
    StatusChanged,
    Assigned,
    Unassigned,
    Deleted,
    Completed
}