using ChronoLog.Core.Models;
using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.Core.Models.DTOs;

namespace ChronoLog.Core.Interfaces;

public interface IWorkdayService
{
    Task<Guid> CreateWorkdayAsync(WorkdayModel workday);
    Task<List<WorkdayViewModel>> GetWorkdaysAsync(DateTime startDate, DateTime endDate);
    Task<List<WorkdayViewModel>> GetWorkdaysAsync();
    Task<WorkdayViewModel?> GetWorkdayByIdAsync(Guid workdayId);
    Task<bool> UpdateWorkdayAsync(Guid workdayId, DateOnly date, WorkdayType type);
    Task<bool> DeleteWorkdayAsync(Guid workdayId);
    Task<TimeSpan> GetTotalWorktimeAsync(Guid workdayId);
    Task<double> GetTotalOvertimeAsync();
    Task<List<WorkdaySummaryResponse>> GetWorkdaySummaryAsync(DateTime startDate, DateTime endDate);
    Task<int> GetOfficeDaysCountAsync(int year);
    Task<int> GetOfficeDaysCountAsync(DateTime startDate, DateTime endDate);
}