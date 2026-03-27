using ChronoLog.Applications.Mappers;
using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.SqlDatabase.Context;
using Microsoft.EntityFrameworkCore;

namespace ChronoLog.Applications.Services;

public class TimeEntryService : ITimeEntryService
{
    private readonly IDbContextFactory<SqlDbContext> _dbContextFactory;
    private readonly IEmployeeContextService _employeeContextService;
    
    public TimeEntryService(IDbContextFactory<SqlDbContext> dbContextFactory, IEmployeeContextService employeeContextService)
    {
        _dbContextFactory = dbContextFactory;
        _employeeContextService = employeeContextService;
    }
    
    public async Task<Guid> CreateTimeEntryAsync(TimeEntryModel timeEntry)
    {
        var model = new TimeEntryModel
        {
            TimeEntryId = Guid.NewGuid(),
            WorkdayId = timeEntry.WorkdayId,
            ProjectId = timeEntry.ProjectId,
            Duration = timeEntry.Duration,
            ResponseText = timeEntry.ResponseText ?? null
        };
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        await sqlDbContext.TimeEntries.AddAsync(model.ToEntity());
        var affectedRows = await sqlDbContext.SaveChangesAsync();
        return affectedRows > 0 ? model.TimeEntryId : Guid.Empty;
    }
    
    public async Task<List<TimeEntryModel>> GetTimeEntriesAsync()
    {
        var employeeId = (await _employeeContextService.GetOrCreateCurrentEmployeeAsync()).EmployeeId;
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        var timeEntries = await sqlDbContext.TimeEntries
            .AsNoTracking()
            .Where(p => p.Workday.EmployeeId == employeeId)
            .Select(p => p.ToModel())
            .ToListAsync();
        return timeEntries;
    }
    
    public async Task<List<TimeEntryModel>> GetTimeEntriesAsync(List<Guid> timeEntryIds)
    {
        if (timeEntryIds.Count == 0)
            return [];
        
        var employeeId = (await _employeeContextService.GetOrCreateCurrentEmployeeAsync()).EmployeeId;
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        var timeEntries = await sqlDbContext.TimeEntries
            .AsNoTracking()
            .Where(p => p.Workday.EmployeeId == employeeId)
            .Where(p => timeEntryIds.Contains(p.TimeEntryId))
            .Select(p => p.ToModel())
            .ToListAsync();
        return timeEntries;
    }
    
    public async Task<List<TimeEntryModel>> GetTimeEntriesAsync(DateTime startDate, DateTime endDate)
    {
        var employeeId = (await _employeeContextService.GetOrCreateCurrentEmployeeAsync()).EmployeeId;
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        var timeEntries = await sqlDbContext.TimeEntries
            .AsNoTracking()
            .Where(p => p.Workday.EmployeeId == employeeId)
            .Where(pt => pt.Workday.Date >= startDate && pt.Workday.Date <= endDate)
            .Select(p => p.ToModel())
            .ToListAsync();
    
        return timeEntries;
    }
    
    public async Task<TimeEntryModel?> GetTimeEntryAsync(Guid timeEntryId)
    {
        var employeeId = (await _employeeContextService.GetOrCreateCurrentEmployeeAsync()).EmployeeId;
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        var timeEntry = await sqlDbContext.TimeEntries
            .AsNoTracking()
            .Where(p => p.Workday.EmployeeId == employeeId)
            .Where(p => p.TimeEntryId == timeEntryId)
            .Select(p => p.ToModel())
            .FirstOrDefaultAsync();
        return timeEntry;
    }
    
    public async Task<bool> UpdateTimeEntryAsync(TimeEntryModel timeEntry)
    {
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        var existingTimeEntry = await sqlDbContext.TimeEntries
            .FirstOrDefaultAsync(p => p.TimeEntryId == timeEntry.TimeEntryId);
        if (existingTimeEntry == null)
            return false;

        existingTimeEntry.WorkdayId = timeEntry.WorkdayId;
        existingTimeEntry.ProjectId = timeEntry.ProjectId;
        existingTimeEntry.Duration = timeEntry.Duration;
        existingTimeEntry.ResponseText = timeEntry.ResponseText ?? null;

        sqlDbContext.TimeEntries.Update(existingTimeEntry);
        var affectedRows = await sqlDbContext.SaveChangesAsync();
        return affectedRows > 0;
    }
    
    public async Task<bool> DeleteTimeEntryAsync(Guid timeEntryId)
    {
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        var existingTimeEntry = await sqlDbContext.TimeEntries
            .FirstOrDefaultAsync(p => p.TimeEntryId == timeEntryId);
        if (existingTimeEntry == null)
            return false;

        sqlDbContext.TimeEntries.Remove(existingTimeEntry);
        var affectedRows = await sqlDbContext.SaveChangesAsync();
        return affectedRows > 0;
    }
}