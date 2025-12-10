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
    public async Task<ActionResult<WorktimeModel>> GetWorktimesById(Guid worktimeId)
    {
        var worktime = await _worktimeService.GetWorktimeByIdAsync(worktimeId);
        if (worktime == null)
            return NotFound("Worktime not found");
        return Ok(worktime);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(WorktimePostModel), 201)]
    public async Task<ActionResult<WorktimeModel>> CreateWorktime([FromBody] WorktimePostModel worktime)
    {
        var createdWorktime = await _worktimeService.CreateWorktimeAsync(worktime);
        return CreatedAtAction(nameof(GetWorktimesById), new { worktimeId = createdWorktime }, createdWorktime);
    }
}