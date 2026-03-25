using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.Core.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChronoLog.ChronoLogService.Controllers;

/// <summary>
/// Managing TimeEntries API Controller: CRUD operations for time entries.
/// </summary>
[Authorize(AuthenticationSchemes = "Bearer,OpenIdConnect")]
[ApiController]
[Route("api/[controller]")]
public class TimeEntryController : ControllerBase
{
    private readonly ITimeEntryService _timeEntryService;
    private readonly IProjectService _projectService;
    
    public TimeEntryController(ITimeEntryService timeEntryService, IProjectService projectService)
    {
        _timeEntryService = timeEntryService;
        _projectService = projectService;
    }
    
    /// <summary>
    /// Creates a new time entry.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>Created TimeEntryModel</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TimeEntryModel), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<TimeEntryModel>> PostNewTimeEntries([FromBody] TimeEntryRequest value)
    {
        var timeEntry = new TimeEntryModel
        {
            TimeEntryId = Guid.NewGuid(),
            WorkdayId = value.WorkdayId,
            ProjectId = value.ProjectId ?? await _projectService.GetDefaultProjectIdAsync(),
            Duration = value.Duration,
            ResponseText = value.ResponseText ?? string.Empty
        };
        var newTimeEntryId = await _timeEntryService.CreateTimeEntryAsync(timeEntry);
        if (newTimeEntryId != Guid.Empty)
            return CreatedAtAction(nameof(GetTimeEntry), new { TimeEntryId = newTimeEntryId }, timeEntry);
        return BadRequest("Failed to create time entry.");
    }
    
    /// <summary>
    /// Returns all time entries.
    /// </summary>
    /// <returns>List of TimeEntryModel</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<TimeEntryModel>), 200)]
    public async Task<ActionResult<List<TimeEntryModel>>> GetTimeEntries()
    {
        var timeEntries = await _timeEntryService.GetTimeEntriesAsync();
        return Ok(timeEntries);
    }
    
    /// <summary>
    /// Returns time entries by a list of IDs.
    /// </summary>
    /// <param name="timeEntryIds"></param>
    /// <returns>List of TimeEntryModel</returns>
    [HttpGet("{timeEntryIds}")]
    [ProducesResponseType(typeof(List<TimeEntryModel>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<List<TimeEntryModel>>> GetTimeEntries([FromRoute] string timeEntryIds)
    {
        var ids = timeEntryIds.Split(',').Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty).Where(guid => guid != Guid.Empty).ToList();
        if (ids.Count == 0)
            return BadRequest("No valid time entry IDs provided.");

        var timeEntries = await _timeEntryService.GetTimeEntriesAsync(ids);
        if (timeEntries.Count > 0)
            return Ok(timeEntries);
        return NotFound();
    }
    
    /// <summary>
    /// Returns time entries within a date range.
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <returns>List of TimeEntryModel</returns>
    [HttpGet("startdate/{startDate}/enddate/{endDate}")]
    [ProducesResponseType(typeof(List<TimeEntryModel>), 200)]
    public async Task<ActionResult<List<TimeEntryModel>>> GetTimeEntries([FromRoute] DateTime startDate, [FromRoute] DateTime endDate)
    {
        var timeEntries = await _timeEntryService.GetTimeEntriesAsync(startDate, endDate);
        return Ok(timeEntries);
    }

    /// <summary>
    /// Returns a time entry by its ID.
    /// </summary>
    /// <param name="timeEntryId"></param>
    /// <returns>TimeEntryModel</returns>
    [HttpGet("{timeEntryId:guid}")]
    [ProducesResponseType(typeof(TimeEntryModel), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TimeEntryModel>> GetTimeEntry([FromRoute] Guid timeEntryId)
    {
        var timeEntry = await _timeEntryService.GetTimeEntryAsync(timeEntryId);
        if (timeEntry != null)
            return Ok(timeEntry);
        return NotFound();
    }

    /// <summary>
    /// Updates an existing time entry.
    /// </summary>
    /// <param name="timeEntryId"></param>
    /// <param name="value"></param>
    [HttpPatch("{timeEntryId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> PatchTimeEntry([FromRoute] Guid timeEntryId,
        [FromBody] TimeEntryUpdateRequest value)
    {
        var existingTimeEntry = await _timeEntryService.GetTimeEntryAsync(timeEntryId);
        if (existingTimeEntry is null)
            return NotFound($"Time Entry with ID {timeEntryId} not found.");
        
        existingTimeEntry.WorkdayId = value.WorkdayId ?? existingTimeEntry.WorkdayId;
        existingTimeEntry.ProjectId = value.ProjectId ?? existingTimeEntry.ProjectId;
        existingTimeEntry.Duration = value.Duration ?? existingTimeEntry.Duration;
        existingTimeEntry.ResponseText = value.ResponseText ?? existingTimeEntry.ResponseText;
        
        var result = await _timeEntryService.UpdateTimeEntryAsync(existingTimeEntry);
        if (result)
            return NoContent();
        return BadRequest("Failed to update time entry.");
    }

    /// <summary>
    /// Deletes a time entry by its ID.
    /// </summary>
    /// <param name="timeEntryId"></param>
    [HttpDelete("{timeEntryId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteTimeEntry([FromRoute] Guid timeEntryId)
    {
        var existingTimeEntry = await _timeEntryService.GetTimeEntryAsync(timeEntryId);
        if (existingTimeEntry is null)
            return NotFound($"Time Entry with ID {timeEntryId} not found.");
        
        var result = await _timeEntryService.DeleteTimeEntryAsync(timeEntryId);
        if (result)
            return NoContent();
        return BadRequest("Failed to delete the time entry.");
    }
}