using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.Core.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChronoLog.ChronoLogService.Controllers;

/// <summary>
/// Managing Projecttimes API Controller: CRUD operations for projecttimes.
/// </summary>
[Authorize(AuthenticationSchemes = "Cookies,Bearer")] 
[ApiController]
[Route("api/[controller]")]
public class ProjecttimeController : ControllerBase
{
    private readonly ILogger<ProjecttimeController> _logger;
    private readonly IProjecttimeService _projecttimeService;
    private readonly IProjectService _projectService;
    
    public ProjecttimeController(ILogger<ProjecttimeController> logger, IProjecttimeService projecttimeService, IProjectService projectService)
    {
        _logger = logger;
        _projecttimeService = projecttimeService;
        _projectService = projectService;
    }
    
    /// <summary>
    /// Creates a new projecttime.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>Created ProjecttimeModel</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ProjecttimeModel), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ProjecttimeModel>> PostNewProjecttime([FromBody] ProjecttimeRequest value)
    {
        var projecttime = new ProjecttimeModel
        {
            ProjecttimeId = Guid.NewGuid(),
            WorkdayId = value.WorkdayId,
            ProjectId = value.ProjectId ?? await _projectService.GetDefaultProjectIdAsync(),
            TimeSpent = value.TimeSpent,
            ResponseText = value.ResponseText ?? string.Empty
        };
        var newProjecttimeId = await _projecttimeService.CreateProjecttimeAsync(projecttime);
        if (newProjecttimeId != Guid.Empty)
            return CreatedAtAction(nameof(GetProjecttime), new { projecttimeId = newProjecttimeId }, projecttime);
        return BadRequest("Failed to create projecttime.");
    }
    
    /// <summary>
    /// Returns all projecttimes.
    /// </summary>
    /// <returns>List of ProjecttimeModel</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProjecttimeModel>), 200)]
    public async Task<ActionResult<List<ProjecttimeModel>>> GetProjecttimes()
    {
        var projecttimes = await _projecttimeService.GetProjecttimesAsync();
        return Ok(projecttimes);
    }
    
    /// <summary>
    /// Returns projecttimes by a list of IDs.
    /// </summary>
    /// <param name="projecttimeIds"></param>
    /// <returns>List of ProjecttimeModel</returns>
    [HttpGet("{projecttimeIds}")]
    [ProducesResponseType(typeof(ProjecttimeModel), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<List<ProjecttimeModel>>> GetProjecttimes([FromRoute] string projecttimeIds)
    {
        var ids = projecttimeIds.Split(',').Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty).Where(guid => guid != Guid.Empty).ToList();
        if (ids.Count == 0)
            return BadRequest("No valid projecttime IDs provided.");

        var projecttimes = await _projecttimeService.GetProjecttimesAsync(ids);
        if (projecttimes.Count > 0)
            return Ok(projecttimes);
        return NotFound();
    }
    
    /// <summary>
    /// Returns projecttimes within a date range.
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <returns>List of ProjecttimeModel</returns>
    [HttpGet("startdate/{startDate}/enddate/{endDate}")]
    [ProducesResponseType(typeof(List<ProjecttimeModel>), 200)]
    public async Task<ActionResult<List<ProjecttimeModel>>> GetProjecttimes([FromRoute] DateTime startDate, [FromRoute] DateTime endDate)
    {
        var projecttimes = await _projecttimeService.GetProjecttimesAsync(startDate, endDate);
        return Ok(projecttimes);
    }

    /// <summary>
    /// Returns a projecttime by its ID.
    /// </summary>
    /// <param name="projecttimeId"></param>
    /// <returns>ProjecttimeModel</returns>
    [HttpGet("{projecttimeId:guid}")]
    [ProducesResponseType(typeof(ProjecttimeModel), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ProjecttimeModel>> GetProjecttime([FromRoute] Guid projecttimeId)
    {
        var projecttime = await _projecttimeService.GetProjecttimeAsync(projecttimeId);
        if (projecttime != null)
            return Ok(projecttime);
        return NotFound();
    }

    /// <summary>
    /// Updates an existing projecttime.
    /// </summary>
    /// <param name="projecttimeId"></param>
    /// <param name="value"></param>
    [HttpPatch("{projecttimeId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> PatchProjecttime([FromRoute] Guid projecttimeId,
        [FromBody] ProjecttimeUpdateRequest value)
    {
        var existingProjecttime = await _projecttimeService.GetProjecttimeAsync(projecttimeId);
        if (existingProjecttime is null)
            return NotFound($"Projecttime with ID {projecttimeId} not found.");
        
        existingProjecttime.WorkdayId = value.WorkdayId ?? existingProjecttime.WorkdayId;
        existingProjecttime.ProjectId = value.ProjectId ?? existingProjecttime.ProjectId;
        existingProjecttime.TimeSpent = value.TimeSpent ?? existingProjecttime.TimeSpent;
        existingProjecttime.ResponseText = value.ResponseText ?? existingProjecttime.ResponseText;
        
        var result = await _projecttimeService.UpdateProjecttimeAsync(existingProjecttime);
        if (result)
            return NoContent();
        return BadRequest("Failed to update projecttime.");
    }

    /// <summary>
    /// Deletes a projecttime by its ID.
    /// </summary>
    /// <param name="projecttimeId"></param>
    [HttpDelete("{projecttimeId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteProjecttime([FromRoute] Guid projecttimeId)
    {
        var existingProjecttime = await _projecttimeService.GetProjecttimeAsync(projecttimeId);
        if (existingProjecttime is null)
            return NotFound($"Projecttime with ID {projecttimeId} not found.");
        
        var result = await _projecttimeService.DeleteProjecttimeAsync(projecttimeId);
        if (result)
            return NoContent();
        return BadRequest("Failed to delete projecttime.");
    }
}