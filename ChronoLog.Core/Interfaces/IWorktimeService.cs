using ChronoLog.Core.Models.DisplayObjects;

namespace ChronoLog.Core.Interfaces;

public interface IWorktimeService
{
    Task<Guid> CreateWorktimeAsync(WorktimeModel worktime);
    Task<List<WorktimeModel>> GetWorktimesAsync();
    Task<List<WorktimeModel>> GetWorktimesAsync(DateTime startDate, DateTime endDate);
    Task<WorktimeModel?> GetWorktimeAsync(Guid worktimeId);
    Task<bool> UpdateWorktimeAsync(WorktimeModel worktime);
    Task<bool> DeleteWorktimeAsync(Guid worktimeId);
    Task<TimeSpan?> GetTotalWorktimeAsync(DateTime startDate, DateTime endDate);
}