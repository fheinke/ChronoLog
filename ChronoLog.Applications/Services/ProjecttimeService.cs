using ChronoLog.Applications.Mappers;
using ChronoLog.Applications.Shared;
using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.SqlDatabase.Context;
using Microsoft.EntityFrameworkCore;

namespace ChronoLog.Applications.Services;

public class ProjecttimeService : IProjecttimeService
{
    private readonly SqlDbContext _sqlDbContext;
    private readonly IEmployeeContextService _employeeContextService;
    
    public ProjecttimeService(SqlDbContext sqlDbContext, IEmployeeContextService employeeContextService)
    {
        _sqlDbContext = sqlDbContext;
        _employeeContextService = employeeContextService;
    }
    
    public async Task<Guid> CreateProjecttimeAsync(ProjecttimeModel projecttime)
    {
        var model = new ProjecttimeModel
        {
            ProjecttimeId = Guid.NewGuid(),
            WorkdayId = projecttime.WorkdayId,
            ProjectId = projecttime.ProjectId,
            TimeSpent = projecttime.TimeSpent,
            ResponseText = projecttime.ResponseText ?? null
        };
        await _sqlDbContext.Projecttimes.AddAsync(model.ToEntity());
        var affectedRows = await _sqlDbContext.SaveChangesAsync();
        return affectedRows > 0 ? model.ProjecttimeId : Guid.Empty;
    }
    
    public async Task<List<ProjecttimeModel>> GetProjecttimesAsync()
    {
        var employeeId = await Helper.GetCurrentEmployeeIdAsync(_employeeContextService);
        var projecttimes = await _sqlDbContext.Projecttimes
            .AsNoTracking()
            .Where(p => p.Workday.EmployeeId == employeeId)
            .Select(p => p.ToModel())
            .ToListAsync();
        return projecttimes;
    }
    
    public async Task<List<ProjecttimeModel>> GetProjecttimesAsync(List<Guid> projecttimeIds)
    {
        if (projecttimeIds.Count == 0)
            return [];
        
        var employeeId = await Helper.GetCurrentEmployeeIdAsync(_employeeContextService);
        var projecttimes = await _sqlDbContext.Projecttimes
            .AsNoTracking()
            .Where(p => p.Workday.EmployeeId == employeeId)
            .Where(p => projecttimeIds.Contains(p.ProjecttimeId))
            .Select(p => p.ToModel())
            .ToListAsync();
        return projecttimes;
    }
    
    public async Task<List<ProjecttimeModel>> GetProjecttimesAsync(DateTime startDate, DateTime endDate)
    {
        var employeeId = await Helper.GetCurrentEmployeeIdAsync(_employeeContextService);
        var projecttimes = await _sqlDbContext.Projecttimes
            .AsNoTracking()
            .Where(p => p.Workday.EmployeeId == employeeId)
            .Where(pt => pt.Workday.Date >= startDate && pt.Workday.Date <= endDate)
            .Select(p => p.ToModel())
            .ToListAsync();
    
        return projecttimes;
    }
    

    public async Task<ProjecttimeModel?> GetProjecttimeAsync(Guid projecttimeId)
    {
        var employeeId = await Helper.GetCurrentEmployeeIdAsync(_employeeContextService);
        var projecttime = await _sqlDbContext.Projecttimes
            .AsNoTracking()
            .Where(p => p.Workday.EmployeeId == employeeId)
            .Where(p => p.ProjecttimeId == projecttimeId)
            .Select(p => p.ToModel())
            .FirstOrDefaultAsync();
        return projecttime;
    }
    
    public async Task<bool> UpdateProjecttimeAsync(ProjecttimeModel projecttime)
    {
        var existingProjecttime = await _sqlDbContext.Projecttimes
            .FirstOrDefaultAsync(p => p.ProjecttimeId == projecttime.ProjecttimeId);
        if (existingProjecttime == null)
            return false;

        existingProjecttime.WorkdayId = projecttime.WorkdayId;
        existingProjecttime.ProjectId = projecttime.ProjectId;
        existingProjecttime.TimeSpent = projecttime.TimeSpent;
        existingProjecttime.ResponseText = projecttime.ResponseText ?? null;

        _sqlDbContext.Projecttimes.Update(existingProjecttime);
        var affectedRows = await _sqlDbContext.SaveChangesAsync();
        return affectedRows > 0;
    }
    
    public async Task<bool> DeleteProjecttimeAsync(Guid projecttimeId)
    {
        var existingProjecttime = await _sqlDbContext.Projecttimes
            .FirstOrDefaultAsync(p => p.ProjecttimeId == projecttimeId);
        if (existingProjecttime == null)
            return false;

        _sqlDbContext.Projecttimes.Remove(existingProjecttime);
        var affectedRows = await _sqlDbContext.SaveChangesAsync();
        return affectedRows > 0;
    }
}