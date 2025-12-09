using ChronoLog.Core.Models.DisplayObjects;
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
}