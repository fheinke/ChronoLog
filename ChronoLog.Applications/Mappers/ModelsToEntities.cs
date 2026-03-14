using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.SqlDatabase.Models;

namespace ChronoLog.Applications.Mappers;

public static class ModelsToEntities
{
    public static ProjectEntity ToEntity(this ProjectModel model)
    {
        return new ProjectEntity
        {
            ProjectId = model.ProjectId,
            IsDefault = model.IsDefault,
            Name = model.Name,
            Description = model.Description,
            ResponseObject = model.ResponseObject,
            DefaultResponseText = model.DefaultResponseText
        };
    }
    
    public static WorkdayEntity ToEntity(this WorkdayModel model)
    {
        return new WorkdayEntity
        {
            WorkdayId = model.WorkdayId,
            EmployeeId =  model.EmployeeId,
            Date = model.Date,
            Type = model.Type
        };
    }

    public static WorktimeEntity ToEntity(this WorktimeModel model)
    {
        return new WorktimeEntity
        {
            WorktimeId = model.WorktimeId,
            WorkdayId = model.WorkdayId,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            BreakTime = model.BreakTime
        };
    }
    
    public static TimeEntryEntity ToEntity(this TimeEntryModel model)
    {
        return new TimeEntryEntity
        {
            TimeEntryId = model.TimeEntryId,
            WorkdayId = model.WorkdayId,
            ProjectId = model.ProjectId,
            Duration = model.Duration,
            ResponseText = model.ResponseText
        };
    }

    public static EmployeeEntity ToEntity(this EmployeeModel model)
    {
        return new EmployeeEntity
        {
            EmployeeId = model.EmployeeId,
            ObjectId = model.ObjectId,
            Email = model.Email,
            Name = model.Name,
            Province = model.Province,
            IsAdmin = model.IsAdmin,
            IsProjectManager = model.IsProjectManager,
            VacationDaysPerYear = model.VacationDaysPerYear,
            DailyWorkingTimeInHours = model.DailyWorkingTimeInHours,
            OvertimeCorrectionInHours = model.OvertimeCorrectionInHours,
            LastSeen = model.LastSeen,
        };
    }
}