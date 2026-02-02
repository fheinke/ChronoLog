using ChronoLog.Applications.Mappers;
using ChronoLog.Applications.Shared;
using ChronoLog.Core;
using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models;
using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.Core.Models.DTOs;
using ChronoLog.SqlDatabase.Context;
using Microsoft.EntityFrameworkCore;

namespace ChronoLog.Applications.Services;

public class WorkdayService : IWorkdayService
{
    private readonly SqlDbContext _sqlDbContext;
    private readonly IEmployeeContextService _employeeContextService;

    public WorkdayService(SqlDbContext sqlDbContext, IEmployeeContextService employeeContextService)
    {
        _sqlDbContext = sqlDbContext;
        _employeeContextService = employeeContextService;
    }

    public async Task<Guid> CreateWorkdayAsync(WorkdayModel workday)
    {
        var model = new WorkdayModel
        {
            WorkdayId = Guid.NewGuid(),
            EmployeeId = await Helper.GetCurrentEmployeeIdAsync(_employeeContextService),
            Date = workday.Date,
            Type = workday.Type
        };
        await _sqlDbContext.Workdays.AddAsync(model.ToEntity());
        var affectedRows = await _sqlDbContext.SaveChangesAsync();
        return affectedRows > 0 ? model.WorkdayId : Guid.Empty;
    }

    public async Task<List<WorkdayViewModel>> GetWorkdaysAsync() =>
        await GetWorkdaysInternalAsync(null, null);

    public async Task<List<WorkdayViewModel>> GetWorkdaysAsync(DateTime startDate, DateTime endDate) =>
        await GetWorkdaysInternalAsync(startDate, endDate);

    private async Task<List<WorkdayViewModel>> GetWorkdaysInternalAsync(DateTime? startDate, DateTime? endDate)
    {
        var employeeId = await Helper.GetCurrentEmployeeIdAsync(_employeeContextService);
        var query = _sqlDbContext.Workdays
            .AsNoTracking()
            .Where(w => w.EmployeeId == employeeId);

        if (startDate.HasValue && endDate.HasValue)
            query = query.Where(w => w.Date >= startDate.Value && w.Date <= endDate.Value);

        var workdays = await query
            .OrderBy(w => w.Date)
            .Include(w => w.Worktimes.OrderBy(x => x.StartTime))
            .Include(w => w.Projecttimes)
            .Select(w => w.ToViewModel())
            .ToListAsync();

        foreach (var workday in workdays)
            workday.Worktimes = workday.Worktimes.OrderBy(x => x.StartTime).ToList();

        return workdays;
    }

    public async Task<WorkdayViewModel?> GetWorkdayByIdAsync(Guid workdayId)
    {
        var workday = await _sqlDbContext.Workdays
            .AsNoTracking()
            .Where(w => w.WorkdayId == workdayId)
            .Include(w => w.Worktimes)
            .Include(w => w.Projecttimes)
            .Select(w => w.ToViewModel())
            .FirstOrDefaultAsync();

        workday?.Worktimes = workday.Worktimes.OrderBy(x => x.StartTime).ToList();

        return workday ?? null;
    }

    public async Task<bool> UpdateWorkdayAsync(Guid workdayId, DateOnly date, WorkdayType type)
    {
        var workday = await _sqlDbContext.Workdays
            .FirstOrDefaultAsync(w => w.WorkdayId == workdayId);
        if (workday == null)
            return false;

        workday.Date = date.ToDateTime(new TimeOnly(0, 0));
        workday.Type = type;
        _sqlDbContext.Workdays.Update(workday);
        var affectedRows = await _sqlDbContext.SaveChangesAsync();
        return affectedRows > 0;
    }

    public async Task<bool> DeleteWorkdayAsync(Guid workdayId)
    {
        var workday = await _sqlDbContext.Workdays
            .FirstOrDefaultAsync(w => w.WorkdayId == workdayId);
        if (workday == null)
            return false;

        _sqlDbContext.Workdays.Remove(workday);
        var affectedRows = await _sqlDbContext.SaveChangesAsync();
        return affectedRows > 0;
    }

    public async Task<TimeSpan> GetTotalWorktimeAsync(Guid workdayId)
    {
        var workday = await _sqlDbContext.Workdays
            .AsNoTracking()
            .Where(w => w.WorkdayId == workdayId)
            .Include(w => w.Worktimes)
            .Select(w => w.ToViewModel())
            .FirstOrDefaultAsync();
        if (workday == null || workday.Worktimes.Count == 0)
            return TimeSpan.Zero;

        var totalWorktime = CalculateDailyWorktime(workday);
        return totalWorktime;
    }

    public async Task<double> GetTotalOvertimeAsync()
    {
        var employee = await Helper.GetCurrentEmployeeAsync(_employeeContextService);
        var workdays = await _sqlDbContext.Workdays
            .AsNoTracking()
            .Include(w => w.Worktimes)
            .Where(wd => wd.EmployeeId == employee.EmployeeId)
            .Select(wd => wd.ToViewModel())
            .ToListAsync();

        var totalOvertime = workdays.Sum(workday => CalculateDailyOvertime(workday, employee.DailyWorkingTimeInHours));
        return totalOvertime + employee.OvertimeCorrectionInHours;
    }

    public async Task<List<WorkdaySummaryResponse>> GetWorkdaySummaryAsync(DateTime startDate, DateTime endDate)
    {
        var employee = await Helper.GetCurrentEmployeeAsync(_employeeContextService);
        var workdays = await _sqlDbContext.Workdays
            .AsNoTracking()
            .Include(w => w.Worktimes)
            .Where(wd => wd.EmployeeId == employee.EmployeeId && wd.Date >= startDate && wd.Date <= endDate)
            .Select(wd => wd.ToViewModel())
            .ToListAsync();

        var workdaySummaries = (from workday in workdays
            let dailyOvertime = CalculateDailyOvertime(workday, employee.DailyWorkingTimeInHours)
            let totalWorktime = CalculateDailyWorktime(workday)
            select new WorkdaySummaryResponse(workday.WorkdayId, DateOnly.FromDateTime(workday.Date), totalWorktime,
                workday.Type, dailyOvertime)).ToList();

        workdaySummaries = workdaySummaries.OrderBy(s => s.Date).ToList();
        return workdaySummaries;
    }

    public async Task<int> GetOfficeDaysCountAsync(int year)
    {
        var employeeId = await Helper.GetCurrentEmployeeIdAsync(_employeeContextService);
        var officeDaysCount = await _sqlDbContext.Workdays
            .AsNoTracking()
            .Where(w => w.EmployeeId == employeeId &&
                        w.Type == WorkdayType.Office &&
                        w.Date.Year == year)
            .CountAsync();
        return officeDaysCount;
    }

    public async Task<int> GetOfficeDaysCountAsync(DateTime startDate, DateTime endDate)
    {
        var employeeId = await Helper.GetCurrentEmployeeIdAsync(_employeeContextService);
        var officeDaysCount = await _sqlDbContext.Workdays
            .AsNoTracking()
            .Where(w => w.EmployeeId == employeeId &&
                        w.Type == WorkdayType.Office &&
                        w.Date >= startDate &&
                        w.Date <= endDate)
            .CountAsync();
        return officeDaysCount;
    }

    public async Task<List<Dictionary<WorkdayType, int>>> GetWorkdayTypeSummaryAsync(int year)
    {
        return await GetWorkdayTypeSummaryInternal(year, null, null);
    }

    public async Task<List<Dictionary<WorkdayType, int>>> GetWorkdayTypeSummaryAsync(DateTime startDate,
        DateTime endDate)
    {
        return await GetWorkdayTypeSummaryInternal(null, startDate, endDate);
    }

    private static TimeSpan CalculateDailyWorktime(WorkdayViewModel workday)
    {
        var totalWorktime = TimeSpan.Zero;

        foreach (var worktime in workday.Worktimes.Where(wt => wt.EndTime.HasValue))
        {
            var duration = worktime.EndTime!.Value - worktime.StartTime;
            totalWorktime += duration;

            if (worktime.BreakTime.HasValue)
                totalWorktime -= worktime.BreakTime.Value;
        }

        return totalWorktime;
    }

    private static double CalculateDailyOvertime(WorkdayViewModel workday, double dailyWorkingTimeInHours)
    {
        var totalOvertime = 0.0;
        if (workday.Type == WorkdayType.Gleitzeittag) return -dailyWorkingTimeInHours;
        if (workday.Worktimes.Count == 0) return totalOvertime;

        foreach (var worktime in workday.Worktimes.Where(wt => wt.EndTime.HasValue))
        {
            var duration = (worktime.EndTime!.Value - worktime.StartTime).TotalHours;
            totalOvertime += duration;

            if (worktime.BreakTime.HasValue)
                totalOvertime -= worktime.BreakTime.Value.TotalHours;
        }

        if (workday.Type.IsNonWorkingDay())
            return totalOvertime;
        return totalOvertime - dailyWorkingTimeInHours;
    }

    private async Task<List<Dictionary<WorkdayType, int>>> GetWorkdayTypeSummaryInternal(int? year, DateTime? startDate,
        DateTime? endDate)
    {
        var employeeId = await Helper.GetCurrentEmployeeIdAsync(_employeeContextService);
        var workdayTypes = Enum.GetValues<WorkdayType>();
        var workdayTypeSummary = new List<Dictionary<WorkdayType, int>>();

        foreach (var workdayType in workdayTypes)
        {
            var query = _sqlDbContext.Workdays
                .AsNoTracking()
                .Where(w => w.EmployeeId == employeeId && w.Type == workdayType);

            query = year.HasValue
                ? query.Where(w => w.Date.Year == year.Value)
                : query.Where(w => w.Date >= startDate && w.Date <= endDate);

            var count = await query.CountAsync();

            workdayTypeSummary.Add(new Dictionary<WorkdayType, int>
            {
                { workdayType, count }
            });
        }

        return workdayTypeSummary;
    }
}