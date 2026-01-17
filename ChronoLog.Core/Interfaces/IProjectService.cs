using ChronoLog.Core.Models.DisplayObjects;

namespace ChronoLog.Core.Interfaces;

public interface IProjectService
{
    Task<bool> CreateProjectAsync(ProjectModel projectModel);
    Task<List<ProjectModel>> GetProjectsAsync();
    Task<ProjectModel?> GetProjectByIdAsync(Guid projectId);
    Task<Guid> GetDefaultProjectIdAsync();

    Task<bool> UpdateProjectAsync(Guid projectId, string? name, string? description, string? responseObject,
        string? defaultResponseText, bool? isDefault);

    Task<bool> UpdateProjectAsync(ProjectModel projectModel);
    Task<bool> DeleteProjectAsync(Guid projectId);
}