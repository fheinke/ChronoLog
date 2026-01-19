using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.Core.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChronoLog.ChronoLogService.Controllers;

/// <summary>
/// Managing Worktimes API Controller: CRUD operations for worktimes.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WorktimeController : ControllerBase
{
    private readonly ILogger<WorktimeController> _logger;
    private readonly IWorktimeService _worktimeService;

    public WorktimeController(ILogger<WorktimeController> logger, IWorktimeService worktimeService)
    {
        _logger = logger;
        _worktimeService = worktimeService;
    }

    /// <summary>
    /// Creates a new Worktime.
    /// </summary>
    /// <param name="worktime"></param>
    /// <returns>Created WorktimeModel</returns>
    [HttpPost]
    [ProducesResponseType(typeof(WorktimeModel), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<WorktimeModel>> PostNewWorktime([FromBody] WorktimeRequest worktime)
    {
        var worktimeModel = new WorktimeModel
        {
            WorkdayId = worktime.WorkdayId,
            StartTime = worktime.StartTime,
            EndTime = worktime.EndTime ?? null,
            BreakTime = worktime.BreakTime ?? null
        };
        var createdWorktime = await _worktimeService.CreateWorktimeAsync(worktimeModel);
        if (createdWorktime != Guid.Empty)
            return CreatedAtAction(nameof(GetWorktimeById), new { worktimeId = createdWorktime }, createdWorktime);
        return BadRequest("Failed to create worktime.");
    }

    /// <summary>
    /// Returns all Worktimes.
    /// </summary>
    /// <returns>List of WorktimeModel</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<WorktimeModel>), 200)]
    public async Task<ActionResult<List<WorktimeModel>>> GetWorktimes()
    {
        var worktimes = await _worktimeService.GetWorktimesAsync();
        return Ok(worktimes);
    }

    /// <summary>
    /// Returns Worktimes within a date range.
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <returns>List of WorktimeModel</returns>
    [HttpGet("startdate/{startDate}/enddate/{endDate}")]
    [ProducesResponseType(typeof(WorktimeModel), 200)]
    public async Task<ActionResult<WorktimeModel>> GetWorktimeById([FromRoute] DateOnly startDate,
        [FromRoute] DateOnly endDate)
    {
        var worktimes = await _worktimeService.GetWorktimesAsync(startDate.ToDateTime(TimeOnly.MinValue),
            endDate.ToDateTime(TimeOnly.MaxValue));
        return Ok(worktimes);
    }

    /// <summary>
    /// Returns a Worktime by its ID.
    /// </summary>
    /// <param name="worktimeId"></param>
    /// <returns>WorktimeModel</returns>
    [HttpGet("{worktimeId:guid}")]
    [ProducesResponseType(typeof(WorktimeModel), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<WorktimeModel>> GetWorktimeById(Guid worktimeId)
    {
        var worktime = await _worktimeService.GetWorktimeAsync(worktimeId);
        if (worktime == null)
            return NotFound($"Worktime with ID {worktimeId} not found");
        return Ok(worktime);
    }

    /// <summary>
    /// Returns the total worktime within a date range.
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <returns>Total worktime as TimeSpan</returns>
    [HttpGet("totalWorktime/startdate/{startDate}/enddate/{endDate}")]
    [ProducesResponseType(typeof(TimeSpan), 200)]
    public async Task<ActionResult<TimeSpan>> GetTotalWorktime([FromRoute] DateOnly startDate,
        [FromRoute] DateOnly endDate)
    {
        var totalWorktime = await _worktimeService.GetTotalWorktimeAsync(startDate.ToDateTime(TimeOnly.MinValue),
            endDate.ToDateTime(TimeOnly.MaxValue));
        return Ok(totalWorktime ?? TimeSpan.Zero);
    }

    /// <summary>
    /// Updates an existing Worktime.
    /// </summary>
    /// <param name="worktimeId"></param>
    /// <param name="value"></param>
    /// <returns>NoContent on success</returns>
    [HttpPatch("{worktimeId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> PatchWorktime(Guid worktimeId, [FromBody] WorktimeUpdateRequest value)
    {
        var existingWorktime = await _worktimeService.GetWorktimeAsync(worktimeId);
        if (existingWorktime == null)
            return NotFound($"Worktime with ID {worktimeId} not found");

        existingWorktime.StartTime = value.StartTime ?? existingWorktime.StartTime;
        existingWorktime.EndTime = value.EndTime ?? existingWorktime.EndTime;
        existingWorktime.BreakTime = value.BreakTime ?? existingWorktime.BreakTime;

        var result = await _worktimeService.UpdateWorktimeAsync(existingWorktime);
        if (result)
            return NoContent();
        return BadRequest("Failed to update worktime.");
    }

    /// <summary>
    /// Deletes a Worktime by its ID.
    /// </summary>
    [HttpDelete("{worktimeId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteWorktime(Guid worktimeId)
    {
        var existingWorktime = await _worktimeService.GetWorktimeAsync(worktimeId);
        if (existingWorktime == null)
            return NotFound($"Worktime with ID {worktimeId} not found");

        var result = await _worktimeService.DeleteWorktimeAsync(worktimeId);
        if (result)
            return NoContent();
        return BadRequest("Failed to delete worktime.");
    }
}