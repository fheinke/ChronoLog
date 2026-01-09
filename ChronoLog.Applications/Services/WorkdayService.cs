using ChronoLog.Applications.Mappers;
using ChronoLog.Applications.Shared;
using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models;
using ChronoLog.Core.Models.DisplayObjects;
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

    public async Task<bool> CreateWorkdayAsync(WorkdayPostModel workday)
    {
        var model = new WorkdayModel
        {
            WorkdayId = Guid.NewGuid(),
            EmployeeId = await Helper.GetCurrentEmployeeIdAsync(_employeeContextService),
            Date = workday.Date ?? DateTime.Now,
            Type = workday.Type
        };
        await _sqlDbContext.Workdays.AddAsync(model.ToEntity());
        var affectedRows = await _sqlDbContext.SaveChangesAsync();
        return affectedRows > 0;
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
            .Include(w => w.Worktimes.OrderBy(x => x.StartTime))
            .Include(w => w.Projecttimes)
            .Select(w => w.ToViewModel())
            .ToListAsync();

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
        var worktimes = await _sqlDbContext.Worktimes
            .AsNoTracking()
            .Include(w => w.Workday)
            .Where(wt => wt.Workday.WorkdayId == workdayId)
            .ToListAsync();

        if (worktimes.Count == 0)
            return TimeSpan.Zero;

        var totalWorktime = TimeSpan.Zero;
        foreach (var worktime in worktimes)
        {
            if (worktime.EndTime.HasValue)
            {
                var duration = worktime.EndTime.Value - worktime.StartTime;
                totalWorktime += duration;
            }

            if (worktime.BreakTime.HasValue)
            {
                totalWorktime -= worktime.BreakTime.Value;
            }
        }

        return totalWorktime;
    }

    public async Task<double> GetTotalOvertimeAsync()
    {
        var employeeId = await Helper.GetCurrentEmployeeIdAsync(_employeeContextService);
        var employee = await _sqlDbContext.Employees
            .AsNoTracking()
            .Where(w => w.EmployeeId == employeeId)
            .Select(e => new { e.DailyWorkingTimeInHours, e.OvertimeCorrectionInHours })
            .FirstOrDefaultAsync();
        if (employee == null)
            return 0.0;

        var worktimes = await _sqlDbContext.Worktimes
            .AsNoTracking()
            .Include(w => w.Workday)
            .Where(wt => wt.Workday.EmployeeId == employeeId)
            .ToListAsync();

        var flexDays = await _sqlDbContext.Workdays
            .AsNoTracking()
            .Where(wd => wd.EmployeeId == employeeId && wd.Type == WorkdayType.Gleitzeittag)
            .CountAsync();

        if (worktimes.Count == 0)
            return employee.OvertimeCorrectionInHours - flexDays * employee.DailyWorkingTimeInHours;

        var totalWorktime = worktimes.Sum(wt =>
        {
            if (!wt.EndTime.HasValue)
                return -employee.DailyWorkingTimeInHours;

            var duration = (wt.EndTime.Value - wt.StartTime).TotalHours;
            var breakTime = wt.BreakTime?.TotalHours ?? 0.0;
            return duration - breakTime - employee.DailyWorkingTimeInHours;
        });

        return totalWorktime + employee.OvertimeCorrectionInHours - flexDays * employee.DailyWorkingTimeInHours;
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
}