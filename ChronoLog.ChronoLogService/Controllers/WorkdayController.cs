using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.Core.Models.DTOs;
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
    
    /// <summary>
    /// Creates a new Workday.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>Created WorkdayModel</returns>
    [HttpPost]
    [ProducesResponseType(typeof(WorkdayModel), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<WorkdayModel>> PostNewWorkday([FromBody] WorkdayRequest value)
    {
        var workday = new WorkdayModel
        {
            EmployeeId = Guid.Empty,
            Date = value.Date,
            Type = value.Type
        };
        var newWorkdayId = await _workdayService.CreateWorkdayAsync(workday);
        if (newWorkdayId != Guid.Empty)
            return CreatedAtAction(nameof(GetWorkdayById), new { workdayId = newWorkdayId }, workday);
        return BadRequest("Failed to create workday.");
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
}