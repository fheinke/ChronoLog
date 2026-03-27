using ChronoLog.Applications.Mappers;
using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.SqlDatabase.Context;
using Microsoft.EntityFrameworkCore;

namespace ChronoLog.Applications.Services;

public class ProjectService : IProjectService
{
    private readonly IDbContextFactory<SqlDbContext> _dbContextFactory;
    
    public ProjectService(IDbContextFactory<SqlDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }
    
    public async Task<bool> CreateProjectAsync(ProjectModel projectModel)
    {
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        if (projectModel.IsDefault)
            await sqlDbContext.Projects
                .Where(p => p.IsDefault)
                .ExecuteUpdateAsync(p => p.SetProperty(x => x.IsDefault, false));
        
        await sqlDbContext.Projects.AddAsync(projectModel.ToEntity());
        var affectedRows = await sqlDbContext.SaveChangesAsync();
        return affectedRows > 0;
    }
    
    public async Task<List<ProjectModel>> GetProjectsAsync()
    {
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        var projects = await sqlDbContext.Projects
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => p.ToModel())
            .ToListAsync();
        return projects;
    }

    public async Task<ProjectModel?> GetProjectByIdAsync(Guid projectId)
    {
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        var project = await sqlDbContext.Projects
            .AsNoTracking()
            .Where(p => p.ProjectId == projectId)
            .Select(p => p.ToModel())
            .FirstOrDefaultAsync();
        return project ?? null;
    }
    
    public async Task<Guid> GetDefaultProjectIdAsync()
    {
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        var defaultProject = await sqlDbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IsDefault);
        return defaultProject?.ProjectId ?? Guid.Empty;
    }
    
    public async Task<bool> UpdateProjectAsync(Guid projectId, string? name, string? description, string? responseObject, string? defaultResponseText, bool? isDefault)
    {
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        var project = await sqlDbContext.Projects
            .FirstOrDefaultAsync(p => p.ProjectId == projectId);
        if (project is null)
            return false;
        
        if (name is not null)
            project.Name = name;
        if (description is not null)
            project.Description = description;
        if (responseObject is not null)
            project.ResponseObject = responseObject;
        if (defaultResponseText is not null)
            project.DefaultResponseText = defaultResponseText;
        if (isDefault is not null)
            project.IsDefault = isDefault.Value;
        
        if (project.IsDefault)
            await sqlDbContext.Projects
                .Where(p => p.IsDefault && p.ProjectId != projectId)
                .ExecuteUpdateAsync(p => p.SetProperty(x => x.IsDefault, false));
        
        sqlDbContext.Projects.Update(project);
        var affectedRows = await sqlDbContext.SaveChangesAsync();
        return affectedRows > 0;
    }
    
    public async Task<bool> UpdateProjectAsync(ProjectModel projectModel)
    {
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        var project = await sqlDbContext.Projects
            .FirstOrDefaultAsync(p => p.ProjectId == projectModel.ProjectId);
        if (project is null)
            return false;
        
        project.Name = projectModel.Name;
        project.Description = projectModel.Description;
        project.ResponseObject = projectModel.ResponseObject;
        project.DefaultResponseText = projectModel.DefaultResponseText;
        project.IsDefault = projectModel.IsDefault;
        
        if (project.IsDefault)
            await sqlDbContext.Projects
                .Where(p => p.IsDefault && p.ProjectId != projectModel.ProjectId)
                .ExecuteUpdateAsync(p => p.SetProperty(x => x.IsDefault, false));
        
        sqlDbContext.Projects.Update(project);
        var affectedRows = await sqlDbContext.SaveChangesAsync();
        return affectedRows > 0;
    }
    
    public async Task<bool> DeleteProjectAsync(Guid projectId)
    {
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        var project = await sqlDbContext.Projects
            .FirstOrDefaultAsync(p => p.ProjectId == projectId);
        
        if (project is null)
            return false;
        if (project.IsDefault)
            return false;
        
        sqlDbContext.Projects.Remove(project);
        var affectedRows = await sqlDbContext.SaveChangesAsync();
        return affectedRows > 0;
    }
}