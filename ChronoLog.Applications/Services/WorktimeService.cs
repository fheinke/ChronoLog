using ChronoLog.Applications.Mappers;
using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.SqlDatabase.Context;
using Microsoft.EntityFrameworkCore;

namespace ChronoLog.Applications.Services;

public class WorktimeService : IWorktimeService
{
    private readonly IDbContextFactory<SqlDbContext> _dbContextFactory;
    private readonly IEmployeeContextService _employeeContextService;
    
    public WorktimeService(IDbContextFactory<SqlDbContext> dbContextFactory, IEmployeeContextService employeeContextService)
    {
        _dbContextFactory = dbContextFactory;
        _employeeContextService = employeeContextService;
    }
    
    public async Task<Guid> CreateWorktimeAsync(WorktimeModel worktime)
    {
        var model = new WorktimeModel
        {
            WorktimeId = Guid.NewGuid(),
            WorkdayId = worktime.WorkdayId,
            StartTime = worktime.StartTime,
            EndTime = worktime.EndTime ?? null,
            BreakTime =  worktime.BreakTime ?? null
        };
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        await sqlDbContext.Worktimes.AddAsync(model.ToEntity());
        var affectedRows = await sqlDbContext.SaveChangesAsync();
        return affectedRows > 0 ? model.WorktimeId : Guid.Empty;
    }
    
    public async Task<List<WorktimeModel>> GetWorktimesAsync()
    {
        var employeeId = (await _employeeContextService.GetOrCreateCurrentEmployeeAsync()).EmployeeId;
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        var worktimes = await sqlDbContext.Worktimes
            .AsNoTracking()
            .Where(w => w.Workday.EmployeeId == employeeId)
            .Select(w => w.ToModel())
            .ToListAsync();
        return worktimes;
    }
    
    public async Task<List<WorktimeModel>> GetWorktimesAsync(DateTime startDate, DateTime endDate)
    {
        var employeeId = (await _employeeContextService.GetOrCreateCurrentEmployeeAsync()).EmployeeId;
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        var worktimes = await sqlDbContext.Worktimes
            .AsNoTracking()
            .Where(w => w.Workday.EmployeeId == employeeId)
            .Where(wt => wt.Workday.Date >= startDate && wt.Workday.Date <= endDate)
            .Select(w => w.ToModel())
            .ToListAsync();
    
        return worktimes;
    }
    
    public async Task<WorktimeModel?> GetWorktimeAsync(Guid worktimeId)
    {
        var employeeId = (await _employeeContextService.GetOrCreateCurrentEmployeeAsync()).EmployeeId;
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        var worktime = await sqlDbContext.Worktimes
            .AsNoTracking()
            .Where(w => w.Workday.EmployeeId == employeeId)
            .Where(w => w.WorktimeId == worktimeId)
            .Select(w => w.ToModel())
            .FirstOrDefaultAsync();
        return worktime;
    }
    
    public async Task<bool> UpdateWorktimeAsync(WorktimeModel worktime)
    {
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        var existingWorktime = await sqlDbContext.Worktimes
            .FirstOrDefaultAsync(w => w.WorktimeId == worktime.WorktimeId);
        if (existingWorktime == null)
            return false;
        
        existingWorktime.WorkdayId = worktime.WorkdayId;
        existingWorktime.StartTime = worktime.StartTime;
        existingWorktime.EndTime = worktime.EndTime ?? null;
        existingWorktime.BreakTime = worktime.BreakTime ?? null;
        
        sqlDbContext.Worktimes.Update(existingWorktime);
        var affectedRows = await sqlDbContext.SaveChangesAsync();
        return affectedRows > 0;
    }
    
    public async Task<bool> DeleteWorktimeAsync(Guid worktimeId)
    {
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        var worktime = await sqlDbContext.Worktimes
            .FirstOrDefaultAsync(w => w.WorktimeId == worktimeId);
        if (worktime == null)
            return false;
        
        sqlDbContext.Worktimes.Remove(worktime);
        var affectedRows = await sqlDbContext.SaveChangesAsync();
        return affectedRows > 0;
    }
    
    public async Task<TimeSpan?> GetTotalWorktimeAsync(DateTime startDate, DateTime endDate)
    {
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        var employeeId = (await _employeeContextService.GetOrCreateCurrentEmployeeAsync()).EmployeeId;
        var worktimes = await sqlDbContext.Worktimes
            .AsNoTracking()
            .Include(w => w.Workday)
            .Where(w => w.Workday.EmployeeId == employeeId)
            .Where(wt => wt.Workday.Date >= startDate && wt.Workday.Date <= endDate)
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
}