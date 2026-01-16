using ChronoLog.Core;
using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models;
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

    /// <summary>
    /// Returns all Workday types.
    /// </summary>
    /// <returns>List of Workday types</returns>
    [HttpGet("types")]
    [ProducesResponseType(typeof(WorkdayTypesResponse), 200)]
    public ActionResult<WorkdayTypesResponse> GetWorkdayTypes()
    {
        var workdayTypes = WorkdayType.GetAll<WorkdayType>();
        var workingDays = workdayTypes.Where(t => t.IsWorkingDay()).ToList();
        var nonWorkingDays = workdayTypes.Where(t => t.IsNonWorkingDay()).ToList();

        var response = new WorkdayTypesResponse(workdayTypes, workingDays, nonWorkingDays);
        return Ok(response);
    }

    /// <summary>
    /// Returns all Workdays.
    /// </summary>
    /// <returns>List of WorkdayModel</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<WorkdayResponse>), 200)]
    public async Task<ActionResult<List<WorkdayResponse>>> GetWorkdays()
    {
        var workdays = await _workdayService.GetWorkdaysAsync();
        var response = workdays.Select(workday => new WorkdayResponse(workday.WorkdayId, workday.EmployeeId,
            workday.Date, workday.Type, workday.Worktimes, workday.Projecttimes)).ToList();
        return Ok(response);
    }

    /// <summary>
    /// Returns Workdays within a specified date range.
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <returns>List of WorkdayModel</returns>
    [HttpGet("startdate/{startDate}/enddate/{endDate}")]
    [ProducesResponseType(typeof(List<WorkdayResponse>), 200)]
    public async Task<ActionResult<List<WorkdayResponse>>> GetWorkdays(DateOnly startDate, DateOnly endDate)
    {
        var workdays = await _workdayService.GetWorkdaysAsync(startDate.ToDateTime(TimeOnly.MinValue),
            endDate.ToDateTime(TimeOnly.MaxValue));
        var response = workdays.Select(workday => new WorkdayResponse(workday.WorkdayId, workday.EmployeeId,
            workday.Date, workday.Type, workday.Worktimes, workday.Projecttimes)).ToList();
        return Ok(response);
    }

    /// <summary>
    /// Returns a Workday by its ID.
    /// </summary>
    /// <param name="workdayId"></param>
    /// <returns>WorkdayModel</returns>
    [HttpGet("{workdayId:guid}")]
    [ProducesResponseType(typeof(WorkdayResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<WorkdayResponse>> GetWorkdayById(Guid workdayId)
    {
        var workday = await _workdayService.GetWorkdayByIdAsync(workdayId);
        if (workday == null)
            return NotFound($"Workday with ID {workdayId} not found.");

        var response = new WorkdayResponse(workday.WorkdayId, workday.EmployeeId,
            workday.Date, workday.Type, workday.Worktimes, workday.Projecttimes);
        return Ok(response);
    }

    /// <summary>
    /// Returns the total worktime for a specific Workday.
    /// </summary>
    /// <param name="workdayId"></param>
    /// <returns>Total worktime as TimeSpan</returns>
    [HttpGet("totalWorktime/{workdayId:guid}")]
    [ProducesResponseType(typeof(TimeSpan), 200)]
    public async Task<ActionResult<TimeSpan>> GetTotalWorktime(Guid workdayId)
    {
        var totalWorktime = await _workdayService.GetTotalWorktimeAsync(workdayId);
        return Ok(totalWorktime);
    }

    /// <summary>
    /// Returns the total overtime across all Workdays.
    /// </summary>
    [HttpGet("totalOvertime")]
    [ProducesResponseType(typeof(double), 200)]
    public async Task<ActionResult<double>> GetTotalOvertime()
    {
        var totalOvertime = await _workdayService.GetTotalOvertimeAsync();
        return Ok(totalOvertime);
    }

    /// <summary>
    /// Returns the total number of office days in a specified year.
    /// </summary>
    /// <param name="year"></param>
    /// <returns>Total number of office days as int</returns>
    [HttpGet("officeDays/{year:int}")]
    [ProducesResponseType(typeof(int), 200)]
    public async Task<ActionResult<int>> GetTotalOfficeDays(int year)
    {
        var totalOfficeDays = await _workdayService.GetOfficeDaysCountAsync(year);
        return Ok(totalOfficeDays);
    }

    /// <summary>
    /// Returns the total number of office days in the current year.
    /// </summary>
    /// <returns>Total number of office days as int</returns>
    [HttpGet("officeDays/currentYear")]
    [ProducesResponseType(typeof(int), 200)]
    public async Task<ActionResult<int>> GetTotalOfficeDays()
    {
        var year = DateTime.Now.Year;
        var totalOfficeDays = await _workdayService.GetOfficeDaysCountAsync(year);
        return Ok(totalOfficeDays);
    }

    /// <summary>
    /// Returns the total number of office days within a specified date range.
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <returns>Total number of office days as int</returns>
    [HttpGet("officeDays/startdate/{startDate}/enddate/{endDate}")]
    [ProducesResponseType(typeof(int), 200)]
    public async Task<ActionResult<int>> GetTotalOfficeDays(DateOnly startDate, DateOnly endDate)
    {
        var totalOfficeDays = await _workdayService.GetOfficeDaysCountAsync(startDate.ToDateTime(TimeOnly.MinValue),
            endDate.ToDateTime(TimeOnly.MaxValue));
        return Ok(totalOfficeDays);
    }

    /// <summary>
    /// Updates an existing Workday.
    /// </summary>
    /// <param name="workdayId"></param>
    /// <param name="value"></param>
    [HttpPatch("{workdayId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> PatchWorkday(Guid workdayId, [FromBody] WorkdayUpdateRequest value)
    {
        var existingWorkday = await _workdayService.GetWorkdayByIdAsync(workdayId);
        if (existingWorkday is null)
            return NotFound($"Workday with ID {workdayId} not found.");

        var result = await _workdayService.UpdateWorkdayAsync(
            workdayId,
            value.Date ?? DateOnly.FromDateTime(existingWorkday.Date),
            value.Type ?? existingWorkday.Type);

        if (result)
            return NoContent();
        return BadRequest("Failed to update workday.");
    }

    /// <summary>
    /// Deletes a Workday by its ID.
    /// </summary>
    /// <param name="workdayId"></param>
    [HttpDelete("{workdayId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteWorkday(Guid workdayId)
    {
        var existingWorkday = await _workdayService.GetWorkdayByIdAsync(workdayId);
        if (existingWorkday is null)
            return NotFound($"Workday with ID {workdayId} not found.");
        var result = await _workdayService.DeleteWorkdayAsync(workdayId);
        if (result)
            return NoContent();
        return BadRequest("Failed to delete workday.");
    }
}