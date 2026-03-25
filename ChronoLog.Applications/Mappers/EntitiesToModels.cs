using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.Core.Models.ViewModels;
using ChronoLog.SqlDatabase.Models;

namespace ChronoLog.Applications.Mappers;

public static class EntitiesToModels
{
    public static ProjectModel ToModel(this ProjectEntity entity)
    {
        return new ProjectModel
        {
            ProjectId = entity.ProjectId,
            IsDefault = entity.IsDefault,
            Name = entity.Name,
            Description = entity.Description,
            ResponseObject = entity.ResponseObject,
            DefaultResponseText = entity.DefaultResponseText
        };
    }

    public static WorkdayViewModel ToViewModel(this WorkdayEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new WorkdayViewModel
        {
            WorkdayId = entity.WorkdayId,
            EmployeeId = entity.EmployeeId,
            Date = entity.Date,
            Type = entity.Type,
            Worktimes = entity.Worktimes.Select(wt => wt.ToModel()).ToList(),
            TimeEntries = entity.TimeEntries.Select(pt => pt.ToModel()).ToList()
        };
    }

    public static WorktimeModel ToModel(this WorktimeEntity entity)
    {
        return new WorktimeModel
        {
            WorktimeId = entity.WorktimeId,
            WorkdayId = entity.WorkdayId,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            BreakTime = entity.BreakTime
        };
    }

    public static TimeEntryModel ToModel(this TimeEntryEntity entity)
    {
        return new TimeEntryModel
        {
            TimeEntryId = entity.TimeEntryId,
            WorkdayId = entity.WorkdayId,
            ProjectId = entity.ProjectId,
            Duration = entity.Duration,
            ResponseText = entity.ResponseText
        };
    }
}