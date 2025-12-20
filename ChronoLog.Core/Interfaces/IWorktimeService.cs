using ChronoLog.Core.Models.DisplayObjects;

namespace ChronoLog.Core.Interfaces;

public interface IWorktimeService
{
    Task<bool> CreateWorktimeAsync(WorktimePostModel worktime);
    Task<Guid> CreateWorktimeAsync(WorktimeModel worktime);
    Task<List<WorktimeModel>> GetWorktimesAsync();
    Task<WorktimeModel?> GetWorktimeByIdAsync(Guid worktimeId);
    Task<bool> UpdateWorktimeAsync(WorktimeModel worktime);
    Task<bool> DeleteWorktimeAsync(Guid worktimeId);
}