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

    public async Task<List<WorkdayViewModel>> GetWorkdaysAsync()
    {
        var employeeId = await Helper.GetCurrentEmployeeIdAsync(_employeeContextService);
        var workdays = await _sqlDbContext.Workdays
            .AsNoTracking()
            .Where(w => w.EmployeeId == employeeId)
            .Include(w => w.Worktimes)
            .Include(w => w.Projecttimes)
            .Select(w => w.ToViewModel())
            .ToListAsync();

        foreach (var workday in workdays.Where(workday => workday.Worktimes.Count != 0))
        {
            workday.Worktimes = workday.Worktimes.OrderBy(x => x.StartTime).ToList();
        }

        return workdays;
    }

    public async Task<List<WorkdayViewModel>> GetWorkdaysAsync(DateTime startDate, DateTime endDate)
    {
        var employeeId = await Helper.GetCurrentEmployeeIdAsync(_employeeContextService);
        var workdays = await _sqlDbContext.Workdays
            .AsNoTracking()
            .Where(w => w.EmployeeId == employeeId)
            .Where(w => w.Date >= startDate && w.Date <= endDate)
            .Include(w => w.Worktimes)
            .Include(w => w.Projecttimes)
            .Select(w => w.ToViewModel())
            .ToListAsync();

        foreach (var workday in workdays.Where(workday => workday.Worktimes.Count != 0))
        {
            workday.Worktimes = workday.Worktimes.OrderBy(x => x.StartTime).ToList();
        }

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
        var employeeDailyWorkingTime = await _sqlDbContext.Employees
            .AsNoTracking()
            .Where(e => e.EmployeeId == employeeId)
            .Select(e => e.DailyWorkingTimeInHours)
            .FirstOrDefaultAsync();

        var worktimes = await _sqlDbContext.Worktimes
            .AsNoTracking()
            .Include(w => w.Workday)
            .Where(wt => wt.Workday.EmployeeId == employeeId)
            .ToListAsync();

        if (worktimes.Count == 0)
            return 0.0;

        var totalWorktime = 0.0;

        foreach (var worktime in worktimes)
        {
            if (worktime.EndTime.HasValue)
            {
                var duration = worktime.EndTime.Value - worktime.StartTime;
                totalWorktime += duration.TotalHours;
            }

            if (worktime.BreakTime.HasValue)
            {
                totalWorktime -= worktime.BreakTime.Value.TotalHours;
            }

            totalWorktime -= employeeDailyWorkingTime;
        }

        return totalWorktime;
    }
}