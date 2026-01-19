using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.Core.Models.DTOs;
using ChronoLog.Core.Models.HelperObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChronoLog.ChronoLogService.Controllers;

/// <summary>
/// Employee Context API Controller: Managing current employee data and settings.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly ILogger<EmployeeController> _logger;
    private readonly IEmployeeContextService _employeeService;

    public EmployeeController(ILogger<EmployeeController> logger, IEmployeeContextService employeeService)
    {
        _logger = logger;
        _employeeService = employeeService;
    }
    
    /// <summary>
    /// Returns the current employee data.
    /// </summary>
    /// <returns>EmployeeResponse</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Guid), 200)]
    public async Task<ActionResult<EmployeeResponse>> GetCurrentEmployee()
    {
        var currentEmployee = await _employeeService.GetOrCreateCurrentEmployeeAsync();
        var result = new EmployeeResponse(
            currentEmployee.EmployeeId,
            currentEmployee.Email,
            currentEmployee.Name,
            currentEmployee.Province,
            currentEmployee.IsAdmin,
            currentEmployee.IsProjectManager,
            currentEmployee.VacationDaysPerYear,
            currentEmployee.DailyWorkingTimeInHours,
            currentEmployee.OvertimeCorrectionInHours,
            currentEmployee.LastSeen
        );
        return Ok(result);
    }

    /// <summary>
    /// Returns the absence days for the current employee for the specified year.
    /// </summary>
    /// <returns>List of AbsenceEntrys</returns>
    [HttpGet("absenceDays/currentYear")]
    [ProducesResponseType(typeof(Guid), 200)]
    public async Task<ActionResult<List<AbsenceEntryModel>>> GetEmployeeAbsenceDays()
    {
        var currentEmployee = await _employeeService.GetOrCreateCurrentEmployeeAsync();
        var absenceDays = await _employeeService.GetEmployeeAbsenceDaysAsync(currentEmployee.EmployeeId, DateTime.Today.Year);
        return Ok(absenceDays);
    }
    
    /// <summary>
    /// Returns the absence days for the current employee for the specified year.
    /// </summary>
    /// <returns>List of AbsenceEntrys</returns>
    [HttpGet("absenceDays/{year:int}")]
    [ProducesResponseType(typeof(Guid), 200)]
    public async Task<ActionResult<List<AbsenceEntryModel>>> GetEmployeeAbsenceDays(int year)
    {
        var currentEmployee = await _employeeService.GetOrCreateCurrentEmployeeAsync();
        var absenceDays = await _employeeService.GetEmployeeAbsenceDaysAsync(currentEmployee.EmployeeId, year);
        return Ok(absenceDays);
    }

    /// <summary>
    /// Returns the number of vacation days taken by the current employee for the specified year.
    /// </summary>
    /// <returns>Number of vacation days taken</returns>
    [HttpGet("vacationDaysTaken/currentYear")]
    [ProducesResponseType(typeof(Guid), 200)]
    public async Task<ActionResult<int>> GetEmployeeVacationDaysTaken()
    {
        var currentEmployee = await _employeeService.GetOrCreateCurrentEmployeeAsync();
        var vacationDaysTaken =
            await _employeeService.GetEmployeeVacationDaysCountAsync(currentEmployee.EmployeeId, DateTime.Today.Year);
        return Ok(vacationDaysTaken);
    }

    /// <summary>
    /// Returns the number of vacation days taken by the current employee for the specified year.
    /// </summary>
    /// <returns>Number of vacation days taken</returns>
    [HttpGet("vacationDaysTaken/{year:int}")]
    [ProducesResponseType(typeof(Guid), 200)]
    public async Task<ActionResult<int>> GetEmployeeVacationDaysTaken(int year)
    {
        var currentEmployee = await _employeeService.GetOrCreateCurrentEmployeeAsync();
        var vacationDaysTaken =
            await _employeeService.GetEmployeeVacationDaysCountAsync(currentEmployee.EmployeeId, year);
        return Ok(vacationDaysTaken);
    }
    
    /// <summary>
    /// Updates the current employee settings.
    /// </summary>
    /// <param name="value"></param>
    [HttpPatch]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> PatchEmployeeSettings([FromBody] EmployeeUpdateRequest value)
    {
        var currentEmployee = await _employeeService.GetOrCreateCurrentEmployeeAsync();
        currentEmployee.Province = value.Province ?? currentEmployee.Province;
        currentEmployee.VacationDaysPerYear = value.VacationDaysPerYear ?? currentEmployee.VacationDaysPerYear;
        currentEmployee.DailyWorkingTimeInHours = value.DailyWorkingTimeInHours ?? currentEmployee.DailyWorkingTimeInHours;
        currentEmployee.OvertimeCorrectionInHours = value.OvertimeCorrectionInHours ?? currentEmployee.OvertimeCorrectionInHours;
        
        var result = await _employeeService.UpdateEmployeeAsync(currentEmployee);
        if (result)
            return NoContent();
        return BadRequest("Failed to update employee settings.");
    }
}