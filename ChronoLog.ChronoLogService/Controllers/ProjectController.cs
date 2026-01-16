using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.Core.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChronoLog.ChronoLogService.Controllers;

/// <summary>
/// Managing Projects API Controller: CRUD operations for projects.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProjectController : ControllerBase
{
    private readonly ILogger<ProjectController> _logger;
    private readonly IProjectService _projectService;
    
    public ProjectController(ILogger<ProjectController> logger, IProjectService projectService)
    {
        _logger = logger;
        _projectService = projectService;
    }
    
    /// <summary>
    /// Creates a new project.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>Created ProjectModel</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ProjectModel), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ProjectModel>> PostNewProject([FromBody] ProjectRequest value)
    {
        var project = new ProjectModel
        {
            ProjectId = Guid.NewGuid(),
            Name = value.Name,
            Description = value.Description ?? string.Empty,
            ResponseObject = value.ResponseObject,
            DefaultResponseText = value.DefaultResponseText ?? string.Empty,
            IsDefault = value.IsDefault ?? false
        };
        var result = await _projectService.CreateProjectAsync(project);
        if (result)
            return CreatedAtAction(nameof(GetProjectById), new { projectId = project.ProjectId }, project);
        return BadRequest("Failed to create project.");
    }
    
    /// <summary>
    /// Returns all projects.
    /// </summary>
    /// <returns>List of ProjectModel</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProjectModel>), 200)]
    public async Task<ActionResult<List<ProjectModel>>> GetAllProjects()
    {
        var projects = await _projectService.GetProjectsAsync();
        return Ok(projects);
    }

    /// <summary>
    /// Returns a project by its ID.
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns>ProjectModel</returns>
    [HttpGet("{projectId:guid}")]
    [ProducesResponseType(typeof(ProjectModel), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ProjectModel>> GetProjectById(Guid projectId)
    {
        var project = await _projectService.GetProjectByIdAsync(projectId);
        if (project is null)
            return NotFound($"Project with ID {projectId} not found.");
        return Ok(project);
    }
    
    /// <summary>
    /// Updates an existing project.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="value"></param>
    [HttpPatch("{projectId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> Patch(Guid projectId, [FromBody] ProjectUpdateRequest value)
    {
        var exists = await _projectService.GetProjectByIdAsync(projectId);
        if (exists is null)
            return NotFound($"Project with ID {projectId} not found.");
        
        var result = await _projectService.UpdateProjectAsync(
            projectId,
            value.Name,
            value.Description,
            value.ResponseObject,
            value.DefaultResponseText,
            value.IsDefault);
        
        if (result)
            return NoContent();
        return BadRequest("Failed to update project.");
    }
    
    /// <summary>
    /// Deletes a project by its ID.
    /// </summary>
    /// <param name="projectId"></param>
    [HttpDelete("{projectId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteProject(Guid projectId)
    {
        var exists = await _projectService.GetProjectByIdAsync(projectId);
        if (exists is null)
            return NotFound($"Project with ID {projectId} not found.");
        
        var result = await _projectService.DeleteProjectAsync(projectId);
        if (result)
            return NoContent();
        return BadRequest("Failed to delete project. It my be the default project.");
    }
}