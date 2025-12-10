using ChronoLog.Core.Models;
using ChronoLog.Core.Models.DisplayObjects;

namespace ChronoLog.Core.Interfaces;

public interface IWorkdayService
{
    Task<bool> CreateWorkdayAsync(WorkdayPostModel workday);
    Task<List<WorkdayViewModel>> GetWorkdaysAsync();
    Task<WorkdayViewModel?> GetWorkdayByIdAsync(Guid workdayId);
    Task<bool> UpdateWorkdayAsync(Guid workdayId, DateOnly date, WorkdayType type);
    Task<bool> DeleteWorkdayAsync(Guid workdayId);
}