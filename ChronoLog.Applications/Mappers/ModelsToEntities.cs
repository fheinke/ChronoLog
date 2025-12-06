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
}