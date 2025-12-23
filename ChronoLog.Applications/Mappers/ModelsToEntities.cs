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
    
    public static ProjecttimeEntity ToEntity(this ProjecttimeModel model)
    {
        return new ProjecttimeEntity
        {
            ProjecttimeId = model.ProjecttimeId,
            WorkdayId = model.WorkdayId,
            ProjectId = model.ProjectId,
            TimeSpent = model.TimeSpent,
            ResponseText = model.ResponseText
        };
    }
}