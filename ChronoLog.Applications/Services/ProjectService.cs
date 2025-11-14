using ChronoLog.Applications.Mappers;
using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.SqlDatabase.Context;
using Microsoft.EntityFrameworkCore;

namespace ChronoLog.Applications.Services;

public class ProjectService : IProjectService
{
    private readonly SqlDbContext _sqlDbContext;
    
    public ProjectService(SqlDbContext sqlDbContext)
    {
        _sqlDbContext = sqlDbContext;
    }
    
    public async Task<bool> CreateProjectAsync()
    {
        throw new NotImplementedException();
    }
    
    public async Task<bool> DeleteProjectAsync()
    {
        throw new NotImplementedException();
    }
    
    public async Task<bool> UpdateProjectAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<List<ProjectModel>> ListProjectsAsync()
    {
        var projects = await _sqlDbContext.Projects
            .AsNoTracking()
            .Select(p => p.ToModel())
            .ToListAsync();
        return projects;
    }
    
    public async Task<List<ProjectModel>> ListProjectsAsync(List<Guid> projectIds)
    {
        var projects = await _sqlDbContext.Projects
            .AsNoTracking()
            .Where(p => projectIds.Contains(p.ProjectId))
            .Select(p => p.ToModel())
            .ToListAsync();
        return projects;
    }

    public async Task<bool> SetDefaultProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        await _sqlDbContext.Projects
            .Where(p => p.IsDefault)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.IsDefault, false), cancellationToken);
        
        await _sqlDbContext.Projects
            .Where(p => p.ProjectId == projectId)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.IsDefault, true), cancellationToken);
        
        var affectedRows = await _sqlDbContext.SaveChangesAsync(cancellationToken);
        return affectedRows > 0;
    }
}