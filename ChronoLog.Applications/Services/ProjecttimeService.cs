using ChronoLog.Applications.Mappers;
using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.SqlDatabase.Context;
using Microsoft.EntityFrameworkCore;

namespace ChronoLog.Applications.Services;

public class ProjecttimeService : IProjecttimeService
{
    private readonly SqlDbContext _sqlDbContext;
    
    public ProjecttimeService(SqlDbContext sqlDbContext)
    {
        _sqlDbContext = sqlDbContext;
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
        var projecttimes = await _sqlDbContext.Projecttimes
            .AsNoTracking()
            .Select(p => p.ToModel())
            .ToListAsync();
        return projecttimes;
    }

    public async Task<ProjecttimeModel?> GetProjecttimeAsync(Guid projecttimeId)
    {
        var projecttime = await _sqlDbContext.Projecttimes
            .AsNoTracking()
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