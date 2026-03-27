using ChronoLog.Applications.Mappers;
using ChronoLog.Core;
using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models;
using ChronoLog.Core.Models.DTOs;
using ChronoLog.Core.Models.HelperObjects;
using ChronoLog.SqlDatabase.Context;
using ChronoLog.SqlDatabase.Models;
using Microsoft.EntityFrameworkCore;

namespace ChronoLog.Applications.Services;

public class EmployeeContextService : IEmployeeContextService
{
    private readonly IUserService _userService;
    private readonly IDbContextFactory<SqlDbContext> _dbContextFactory;
    private EmployeeDto? _cachedEmployee;

    public EmployeeContextService(IUserService userService, IDbContextFactory<SqlDbContext> dbContextFactory)
    {
        _userService = userService;
        _dbContextFactory = dbContextFactory;
    }

    private async Task<EmployeeDto?> GetCurrentEmployeeAsync()
    {
        if (_cachedEmployee is not null)
            return _cachedEmployee;

        _cachedEmployee = await FetchAndUpdateEmployeeAsync();
        return _cachedEmployee;
    }

    public async Task<EmployeeDto> GetOrCreateCurrentEmployeeAsync()
    {
        var employee = await GetCurrentEmployeeAsync();
        if (employee is not null)
            return employee;

        var userId = await _userService.GetUserIdAsync();

        var newEmployee = new EmployeeEntity
        {
            EmployeeId = Guid.NewGuid(),
            ObjectId = userId!,
            Email = await _userService.GetUserEmailAsync() ?? "",
            Name = await _userService.GetUserNameAsync() ?? "",
            Province = GermanProvince.ALL,
            VacationDaysPerYear = 30,
            DailyWorkingTimeInHours = 8.0,
            LastSeen = DateTime.UtcNow,
        };

        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        await sqlDbContext.Employees.AddAsync(newEmployee);
        await sqlDbContext.SaveChangesAsync();

        _cachedEmployee = newEmployee.ToDto();
        return _cachedEmployee;
    }

    public async Task<List<EmployeeDto>> GetAllEmployeesAsync()
    {
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        var employees = await sqlDbContext.Employees
            .AsNoTracking()
            .OrderBy(e => e.Email)
            .ToListAsync();
        return employees.Select(e => e.ToDto()).ToList();
    }

    public async Task<List<AbsenceEntryModel>> GetEmployeeAbsenceDaysAsync(Guid employeeId, int year)
    {
        List<AbsenceEntryModel> absenceDays = [];

        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        var workdays = await sqlDbContext.Workdays
            .AsNoTracking()
            .Where(w =>
                w.EmployeeId == employeeId &&
                w.Date.Year == year)
            .ToListAsync();

        var absenceEntries = workdays
            .Where(w => w.Type.IsNonWorkingDay())
            .OrderBy(w => w.Date)
            .ToList();

        if (absenceEntries.Count == 0)
            return [];

        var currentGroup = new List<WorkdayEntity> { absenceEntries[0] };
        for (var i = 1; i < absenceEntries.Count; i++)
        {
            var currentEntry = absenceEntries[i];
            var lastEntryInGroup = absenceEntries[i - 1];

            if ((currentEntry.Date - lastEntryInGroup.Date).Days == 1)
            {
                currentGroup.Add(currentEntry);
            }
            else
            {
                absenceDays.Add(new AbsenceEntryModel
                {
                    StartDate = currentGroup[0].Date,
                    EndDate = currentGroup[^1].Date,
                    AbsenceTypes = string.Join(", ", currentGroup.Select(e => e.Type.ToString()).Distinct()),
                    DurationInDays = currentGroup.Count
                });

                currentGroup = [currentEntry];
            }
        }

        absenceDays.Add(new AbsenceEntryModel
        {
            StartDate = currentGroup[0].Date,
            EndDate = currentGroup[^1].Date,
            AbsenceTypes = string.Join(", ", currentGroup.Select(e => e.Type.ToString()).Distinct()),
            DurationInDays = currentGroup.Count
        });

        return absenceDays;
    }

    public async Task<int> GetEmployeeVacationDaysCountAsync(Guid employeeId, int year)
    {
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        var vacationDays = await sqlDbContext.Workdays
            .Where(w => w.EmployeeId == employeeId)
            .Where(w => w.Date.Year == year)
            .Where(w => w.Type == WorkdayType.Urlaub)
            .CountAsync();

        return vacationDays;
    }

    public async Task<bool> UpdateEmployeeAsync(EmployeeDto employee)
    {
        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        var existingEmployee = await sqlDbContext.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == employee.EmployeeId);

        if (existingEmployee is null)
            return false;

        existingEmployee.Province = employee.Province;
        existingEmployee.VacationDaysPerYear = employee.VacationDaysPerYear;
        existingEmployee.DailyWorkingTimeInHours = employee.DailyWorkingTimeInHours;
        existingEmployee.OvertimeCorrectionInHours = employee.OvertimeCorrectionInHours;

        return await sqlDbContext.SaveChangesAsync() > 0;
    }

    private async Task<EmployeeDto?> FetchAndUpdateEmployeeAsync()
    {
        var userId = await _userService.GetUserIdAsync();
        if (string.IsNullOrEmpty(userId))
            return null;

        await using var sqlDbContext = await _dbContextFactory.CreateDbContextAsync();
        var employee = await sqlDbContext.Employees
            .FirstOrDefaultAsync(e => e.ObjectId == userId);

        if (employee is null)
            return null;
        
        var currentName = await _userService.GetUserNameAsync();
        if (!string.IsNullOrEmpty(currentName) && employee.Name != currentName)
            employee.Name = currentName;

        employee.LastSeen = DateTime.UtcNow;
        await sqlDbContext.SaveChangesAsync();

        return employee.ToDto();
    }
}