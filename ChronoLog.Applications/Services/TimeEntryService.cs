using ChronoLog.Applications.Mappers;
using ChronoLog.Applications.Shared;
using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.SqlDatabase.Context;
using Microsoft.EntityFrameworkCore;

namespace ChronoLog.Applications.Services;

public class TimeEntryService : ITimeEntryService
{
    private readonly SqlDbContext _sqlDbContext;
    private readonly IEmployeeContextService _employeeContextService;
    
    public TimeEntryService(SqlDbContext sqlDbContext, IEmployeeContextService employeeContextService)
    {
        _sqlDbContext = sqlDbContext;
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
        await _sqlDbContext.TimeEntries.AddAsync(model.ToEntity());
        var affectedRows = await _sqlDbContext.SaveChangesAsync();
        return affectedRows > 0 ? model.TimeEntryId : Guid.Empty;
    }
    
    public async Task<List<TimeEntryModel>> GetTimeEntriesAsync()
    {
        var employeeId = await Helper.GetCurrentEmployeeIdAsync(_employeeContextService);
        var timeEntries = await _sqlDbContext.TimeEntries
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
        
        var employeeId = await Helper.GetCurrentEmployeeIdAsync(_employeeContextService);
        var timeEntries = await _sqlDbContext.TimeEntries
            .AsNoTracking()
            .Where(p => p.Workday.EmployeeId == employeeId)
            .Where(p => timeEntryIds.Contains(p.TimeEntryId))
            .Select(p => p.ToModel())
            .ToListAsync();
        return timeEntries;
    }
    
    public async Task<List<TimeEntryModel>> GetTimeEntriesAsync(DateTime startDate, DateTime endDate)
    {
        var employeeId = await Helper.GetCurrentEmployeeIdAsync(_employeeContextService);
        var timeEntries = await _sqlDbContext.TimeEntries
            .AsNoTracking()
            .Where(p => p.Workday.EmployeeId == employeeId)
            .Where(pt => pt.Workday.Date >= startDate && pt.Workday.Date <= endDate)
            .Select(p => p.ToModel())
            .ToListAsync();
    
        return timeEntries;
    }
    
    public async Task<TimeEntryModel?> GetTimeEntryAsync(Guid timeEntryId)
    {
        var employeeId = await Helper.GetCurrentEmployeeIdAsync(_employeeContextService);
        var timeEntry = await _sqlDbContext.TimeEntries
            .AsNoTracking()
            .Where(p => p.Workday.EmployeeId == employeeId)
            .Where(p => p.TimeEntryId == timeEntryId)
            .Select(p => p.ToModel())
            .FirstOrDefaultAsync();
        return timeEntry;
    }
    
    public async Task<bool> UpdateTimeEntryAsync(TimeEntryModel timeEntry)
    {
        var existingTimeEntry = await _sqlDbContext.TimeEntries
            .FirstOrDefaultAsync(p => p.TimeEntryId == timeEntry.TimeEntryId);
        if (existingTimeEntry == null)
            return false;

        existingTimeEntry.WorkdayId = timeEntry.WorkdayId;
        existingTimeEntry.ProjectId = timeEntry.ProjectId;
        existingTimeEntry.Duration = timeEntry.Duration;
        existingTimeEntry.ResponseText = timeEntry.ResponseText ?? null;

        _sqlDbContext.TimeEntries.Update(existingTimeEntry);
        var affectedRows = await _sqlDbContext.SaveChangesAsync();
        return affectedRows > 0;
    }
    
    public async Task<bool> DeleteTimeEntryAsync(Guid timeEntryId)
    {
        var existingTimeEntry = await _sqlDbContext.TimeEntries
            .FirstOrDefaultAsync(p => p.TimeEntryId == timeEntryId);
        if (existingTimeEntry == null)
            return false;

        _sqlDbContext.TimeEntries.Remove(existingTimeEntry);
        var affectedRows = await _sqlDbContext.SaveChangesAsync();
        return affectedRows > 0;
    }
}