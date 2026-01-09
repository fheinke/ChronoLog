using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models.DisplayObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChronoLog.ChronoLogService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WorkdayController : ControllerBase
{
    private readonly ILogger<WorkdayController> _logger;
    private readonly IWorkdayService _workdayService;
    
    public WorkdayController(ILogger<WorkdayController> logger, IWorkdayService workdayService)
    {
        _logger = logger;
        _workdayService = workdayService;
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(List<WorkdayViewModel>), 200)]
    public async Task<ActionResult<List<WorkdayViewModel>>> GetWorkdays()
    {
        var workdays = await _workdayService.GetWorkdaysAsync();
        return Ok(workdays);
    }
    
    [HttpGet("{workdayId}")]
    [ProducesResponseType(typeof(WorkdayViewModel), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<WorkdayViewModel>> GetWorkdayById(Guid workdayId)
    {
        var workday = await _workdayService.GetWorkdayByIdAsync(workdayId);
        if (workday == null)
            return NotFound("Workday not found");
        return Ok(workday);
    }

    [HttpPost]
    [ProducesResponseType(typeof(WorkdayPostModel), 201)]
    public async Task<ActionResult<WorkdayViewModel>> CreateWorkday([FromBody] WorkdayPostModel workday)
    {
        var createdWorkday = await _workdayService.CreateWorkdayAsync(workday);
        return CreatedAtAction(nameof(GetWorkdayById), new { workdayId = createdWorkday }, createdWorkday);
    }
}