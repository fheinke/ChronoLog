using ChronoLog.Applications.Mappers;
using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models;
using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.SqlDatabase.Context;
using Microsoft.EntityFrameworkCore;

namespace ChronoLog.Applications.Services;

public class WorkdayService : IWorkdayService
{
    private readonly SqlDbContext _sqlDbContext;
    
    public WorkdayService(SqlDbContext sqlDbContext)
    {
        _sqlDbContext = sqlDbContext;
    }

    public async Task<bool> CreateWorkdayAsync(WorkdayPostViewModel workday)
    {
        var model = new WorkdayModel
        {
            WorkdayId = Guid.NewGuid(),
            Date = workday.Date ?? DateOnly.FromDateTime(DateTime.Now),
            Type = workday.Type
        };
        await _sqlDbContext.Workdays.AddAsync(model.ToEntity());
        var affectedRows = await _sqlDbContext.SaveChangesAsync();
        return affectedRows > 0;
    }
    
    public async Task<List<WorkdayViewModel>> GetWorkdaysAsync()
    {
        var workdays = await _sqlDbContext.Workdays
            .AsNoTracking()
            .Select(w => w.ToViewModel())
            .ToListAsync();
        return workdays;
    }

    public async Task<WorkdayViewModel?> GetWorkdayByIdAsync(Guid workdayId)
    {
        var workday = await _sqlDbContext.Workdays
            .AsNoTracking()
            .Where(w => w.WorkdayId == workdayId)
            .Select(w => w.ToViewModel())
            .FirstOrDefaultAsync();
        return workday ?? null;
    }

    public async Task<bool> UpdateWorkdayAsync(Guid workdayId, DateOnly date, WorkdayType type)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteWorkdayAsync(Guid workdayId)
    {
        throw new NotImplementedException();
    }
}