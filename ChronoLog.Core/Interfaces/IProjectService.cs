using ChronoLog.Core.Models.DisplayObjects;

namespace ChronoLog.Core.Interfaces;

public interface IProjectService
{
    Task<bool> CreateProjectAsync();
    Task<bool> DeleteProjectAsync();
    Task<bool> UpdateProjectAsync();
    Task<List<ProjectModel>> ListProjectsAsync();
    Task<List<ProjectModel>> ListProjectsAsync(List<Guid> projectIds);
    Task<bool> SetDefaultProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
}
