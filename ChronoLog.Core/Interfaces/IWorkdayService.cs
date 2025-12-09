using ChronoLog.Core.Models;
using ChronoLog.Core.Models.DisplayObjects;

namespace ChronoLog.Core.Interfaces;

public interface IWorkdayService
{
    Task<bool> CreateWorkdayAsync();
    Task<List<WorkdayViewModel>> GetWorkdaysAsync();
    Task<WorkdayModel?> GetWorkdayByIdAsync(Guid workdayId);
    Task<bool> UpdateWorkdayAsync(Guid workdayId, DateOnly date, WorkdayType type);
    Task<bool> DeleteWorkdayAsync(Guid workdayId);
}