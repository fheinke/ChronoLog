using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models.DisplayObjects;
using Microsoft.AspNetCore.Mvc;

namespace ChronoLog.ChronoLogService.Controllers;

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
}