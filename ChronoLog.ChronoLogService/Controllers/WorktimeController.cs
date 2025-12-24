using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models.DisplayObjects;
using Microsoft.AspNetCore.Mvc;

namespace ChronoLog.ChronoLogService.Controllers;

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
    
    [HttpGet]
    [ProducesResponseType(typeof(List<WorktimeModel>), 200)]
    public async Task<ActionResult<List<WorktimeModel>>> GetWorktimes()
    {
        var worktimes = await _worktimeService.GetWorktimesAsync();
        return Ok(worktimes);
    }
    
    [HttpGet("{worktimeId}")]
    [ProducesResponseType(typeof(WorktimeModel), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<WorktimeModel>> GetWorktimeById(Guid worktimeId)
    {
        var worktime = await _worktimeService.GetWorktimeAsync(worktimeId);
        if (worktime == null)
            return NotFound("Worktime not found");
        return Ok(worktime);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(WorktimePostModel), 201)]
    public async Task<ActionResult<WorktimeModel>> CreateWorktime([FromBody] WorktimePostModel worktime)
    {
        var createdWorktime = await _worktimeService.CreateWorktimeAsync(worktime);
        return CreatedAtAction(nameof(GetWorktimeById), new { worktimeId = createdWorktime }, createdWorktime);
    }

    [HttpPut("{worktimeId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateWorktime(Guid worktimeId, [FromBody] WorktimeModel worktime)
    {
        if (worktimeId != worktime.WorktimeId)
            return BadRequest("Worktime ID mismatch");
        var updated = await _worktimeService.UpdateWorktimeAsync(worktime);
        if (!updated)
            return NotFound("Worktime not found");
        return NoContent();
    }

    [HttpDelete("{worktimeId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteWorktime(Guid worktimeId)
    {
        var deleted = await _worktimeService.DeleteWorktimeAsync(worktimeId);
        if (!deleted)
            return NotFound("Worktime not found");
        return NoContent();
    }
}