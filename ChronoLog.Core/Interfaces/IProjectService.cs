using ChronoLog.Core.Models.DisplayObjects;

namespace ChronoLog.Core.Interfaces;

public interface IProjectService
{
    Task<bool> CreateProjectAsync(string name, string description, string responseObject, string defaultResponseText,
        bool? isDefault = false);

    Task<bool> CreateProjectAsync(ProjectModel projectModel);
    Task<List<ProjectModel>> GetProjectsAsync();
    Task<ProjectModel?> GetProjectByIdAsync(Guid projectId);
    Task<bool> UpdateProjectAsync(Guid projectId, string? description, string? responseObject, string? defaultResponseText, bool? isDefault);
    Task<bool> UpdateProjectAsync(ProjectModel projectModel);
    Task<bool> DeleteProjectAsync(Guid projectId);
    Task<bool> SetDefaultProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
}
