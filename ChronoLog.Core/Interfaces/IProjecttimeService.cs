using ChronoLog.Core.Models.DisplayObjects;

namespace ChronoLog.Core.Interfaces;

public interface IProjecttimeService
{
    Task<Guid> CreateProjecttimeAsync(ProjecttimeModel projecttime);
    Task<List<ProjecttimeModel>> GetProjecttimesAsync();
    Task<List<ProjecttimeModel>> GetProjecttimesAsync(List<Guid> projecttimeIds);
    Task<List<ProjecttimeModel>> GetProjecttimesAsync(DateTime startDate, DateTime endDate);
    Task<ProjecttimeModel?> GetProjecttimeAsync(Guid projecttimeId);
    Task<bool> UpdateProjecttimeAsync(ProjecttimeModel projecttime);
    Task<bool> DeleteProjecttimeAsync(Guid projecttimeId);
}