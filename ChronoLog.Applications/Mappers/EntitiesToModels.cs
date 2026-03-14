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
    
    public static WorkdayModel ToModel(this WorkdayEntity entity)
    {
        return new WorkdayModel
        {
            WorkdayId = entity.WorkdayId,
            EmployeeId =  entity.EmployeeId,
            Date = entity.Date,
            Type = entity.Type
        };
    }
    
    public static WorkdayViewModel ToViewModel(this WorkdayEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new WorkdayViewModel
        {
            WorkdayId = entity.WorkdayId,
            EmployeeId =  entity.EmployeeId,
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
    
    public static EmployeeModel ToModel(this EmployeeEntity entity)
    {
        return new EmployeeModel
        {
            EmployeeId = entity.EmployeeId,
            ObjectId = entity.ObjectId,
            Email = entity.Email,
            Name = entity.Name,
            Province = entity.Province,
            IsAdmin = entity.IsAdmin,
            IsProjectManager = entity.IsProjectManager,
            VacationDaysPerYear = entity.VacationDaysPerYear,
            DailyWorkingTimeInHours = entity.DailyWorkingTimeInHours,
            OvertimeCorrectionInHours = entity.OvertimeCorrectionInHours,
            LastSeen = entity.LastSeen
        };
    }
}