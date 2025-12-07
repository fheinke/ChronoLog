using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models.DisplayObjects;
using Microsoft.AspNetCore.Mvc;

namespace ChronoLog.ChronoLogService.Controllers;

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
    
    [HttpGet]
    [ProducesResponseType(typeof(List<ProjectModel>), 200)]
    public async Task<ActionResult<List<ProjectModel>>> GetAllProjects()
    {
        var projects = await _projectService.ListProjectsAsync();
        return Ok(projects);
    }

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
    
    [HttpPost]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> PostNewProject([FromBody] ProjectPostViewModel value)
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
        var result = await _projectService.CreateProjectAsync(
            project.Name,
            project.Description,
            project.ResponseObject,
            project.DefaultResponseText,
            project.IsDefault);
        if (result)
            return Ok("Project created successfully.");
        return BadRequest("Failed to create project.");
    }
    
    [HttpPut("{projectId:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Put(Guid projectId, [FromBody] ProjectPostViewModel value)
    {
        var result = await _projectService.UpdateProjectAsync(
            projectId,
            value.Description ?? null,
            value.ResponseObject,
            value.DefaultResponseText ?? null,
            value.IsDefault ?? false);
        if (result)
            return Ok("Project updated successfully.");
        return BadRequest("Failed to update project.");
    }
    
    [HttpPatch("{projectId:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> PatchDefaultProject(Guid projectId)
    {
        var result = await _projectService.SetDefaultProjectAsync(projectId);
        if (result)
            return Ok("Project set as default successfully.");
        return BadRequest("Failed to set project as default.");
    }
    
    [HttpDelete("{projectId:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> DeleteProject(Guid projectId)
    {
        var result = await _projectService.DeleteProjectAsync(projectId);
        if (result)
            return Ok("Project deleted successfully.");
        return BadRequest("Failed to delete project.");
    }
}