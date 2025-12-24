using ChronoLog.Applications.Mappers;
using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.SqlDatabase.Context;
using Microsoft.EntityFrameworkCore;

namespace ChronoLog.Applications.Services;

public class WorktimeService : IWorktimeService
{
    private readonly SqlDbContext _sqlDbContext;
    
    public WorktimeService(SqlDbContext sqlDbContext)
    {
        _sqlDbContext = sqlDbContext;
    }
    
    public async Task<bool> CreateWorktimeAsync(WorktimePostModel worktime)
    {
        var model = new WorktimeModel
        {
            WorktimeId = Guid.NewGuid(),
            WorkdayId = worktime.WorkdayId,
            StartTime = worktime.StartTime,
            EndTime = worktime.EndTime ?? null,
            BreakTime =  worktime.BreakTime ?? null
        };
        await _sqlDbContext.Worktimes.AddAsync(model.ToEntity());
        var affectedRows = await _sqlDbContext.SaveChangesAsync();
        return affectedRows > 0;
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
        await _sqlDbContext.Worktimes.AddAsync(model.ToEntity());
        var affectedRows = await _sqlDbContext.SaveChangesAsync();
        return affectedRows > 0 ? model.WorktimeId : Guid.Empty;
    }
    
    public async Task<List<WorktimeModel>> GetWorktimesAsync()
    {
        var worktimes = await _sqlDbContext.Worktimes
            .AsNoTracking()
            .Select(w => w.ToModel())
            .ToListAsync();
        return worktimes;
    }
    
    public async Task<List<WorktimeModel>> GetWorktimesAsync(List<Guid> worktimeIds)
    {
        if (worktimeIds.Count == 0)
            return [];
        
        var worktimes = await _sqlDbContext.Worktimes
            .AsNoTracking()
            .Where(w => worktimeIds.Contains(w.WorktimeId))
            .Select(w => w.ToModel())
            .ToListAsync();
        return worktimes;
    }
    
    public async Task<WorktimeModel?> GetWorktimeAsync(Guid worktimeId)
    {
        var worktime = await _sqlDbContext.Worktimes
            .AsNoTracking()
            .Where(w => w.WorktimeId == worktimeId)
            .Select(w => w.ToModel())
            .FirstOrDefaultAsync();
        return worktime;
    }
    
    public async Task<bool> UpdateWorktimeAsync(WorktimeModel worktime)
    {
        var existingWorktime = await _sqlDbContext.Worktimes
            .FirstOrDefaultAsync(w => w.WorktimeId == worktime.WorktimeId);
        if (existingWorktime == null)
            return false;
        
        existingWorktime.WorkdayId = worktime.WorkdayId;
        existingWorktime.StartTime = worktime.StartTime;
        existingWorktime.EndTime = worktime.EndTime ?? null;
        existingWorktime.BreakTime = worktime.BreakTime ?? null;
        
        _sqlDbContext.Worktimes.Update(existingWorktime);
        var affectedRows = await _sqlDbContext.SaveChangesAsync();
        return affectedRows > 0;
    }
    
    public async Task<bool> DeleteWorktimeAsync(Guid worktimeId)
    {
        var worktime = await _sqlDbContext.Worktimes
            .FirstOrDefaultAsync(w => w.WorktimeId == worktimeId);
        if (worktime == null)
            return false;
        
        _sqlDbContext.Worktimes.Remove(worktime);
        var affectedRows = await _sqlDbContext.SaveChangesAsync();
        return affectedRows > 0;
    }
}