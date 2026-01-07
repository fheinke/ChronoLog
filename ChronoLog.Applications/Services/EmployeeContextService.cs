using ChronoLog.Applications.Mappers;
using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models;
using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.SqlDatabase.Context;
using ChronoLog.SqlDatabase.Models;
using Microsoft.EntityFrameworkCore;

namespace ChronoLog.Applications.Services;

public class EmployeeContextService : IEmployeeContextService
{
    private readonly IUserService _userService;
    private readonly SqlDbContext _sqlDbContext;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private EmployeeModel? _cachedEmployee;

    public EmployeeContextService(IUserService userService, SqlDbContext sqlDbContext)
    {
        _userService = userService;
        _sqlDbContext = sqlDbContext;
    }

    private async Task<EmployeeModel?> GetCurrentEmployeeAsync()
    {
        if (_cachedEmployee is not null)
            return _cachedEmployee;

        await _initializationLock.WaitAsync();
        try
        {
            // Double-checked locking
            if (_cachedEmployee is not null)
                return _cachedEmployee;

            _cachedEmployee = await FetchAndUpdateEmployeeAsync();
            return _cachedEmployee;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public async Task<EmployeeModel> GetOrCreateCurrentEmployeeAsync()
    {
        var employee = await GetCurrentEmployeeAsync();
        if (employee is not null)
            return employee;

        await _initializationLock.WaitAsync();
        try
        {
            // Double-checked locking
            if (_cachedEmployee is not null)
                return _cachedEmployee;

            var userId = await _userService.GetUserIdAsync();

            var newEmployee = new EmployeeEntity
            {
                EmployeeId = Guid.NewGuid(),
                ObjectId = userId!,
                Email = await _userService.GetUserEmailAsync() ?? "",
                Name = await _userService.GetUserNameAsync() ?? "",
                Province = GermanProvince.ALL,
                VacationDaysPerYear = 30,
                LastSeen = DateTime.UtcNow,
            };

            await _sqlDbContext.Employees.AddAsync(newEmployee);
            await _sqlDbContext.SaveChangesAsync();

            _cachedEmployee = newEmployee.ToModel();
            return _cachedEmployee;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    private async Task<EmployeeModel?> FetchAndUpdateEmployeeAsync()
    {
        var userId = await _userService.GetUserIdAsync();
        if (string.IsNullOrEmpty(userId))
            return null;

        var employee = await _sqlDbContext.Employees
            .FirstOrDefaultAsync(e => e.ObjectId == userId);

        if (employee is null)
            return null;

        // Check if Name has changed
        var currentName = await _userService.GetUserNameAsync();
        if (!string.IsNullOrEmpty(currentName) && employee.Name != currentName)
            employee.Name = currentName;

        employee.LastSeen = DateTime.UtcNow;
        await _sqlDbContext.SaveChangesAsync();

        return employee.ToModel();
    }

    public async Task<List<AbsenceEntryModel>> GetEmployeeAbsenceDaysAsync(Guid employeeId, int year)
    {
        var reducingWorkdayTypes = Enum.GetValues<ReducingWorkdayType>()
            .Select(w => w.ToString())
            .ToList();

        var absenceEntries = await _sqlDbContext.Workdays
            .Where(w => w.EmployeeId == employeeId)
            .Where(w => w.Date.Year == year)
            .ToListAsync();

        absenceEntries = absenceEntries
            .Where(w => reducingWorkdayTypes.Contains(w.Type.ToString()))
            .OrderBy(w => w.Date)
            .ToList();

        if (absenceEntries.Count == 0)
            return [];

        List<AbsenceEntryModel> absenceDays = [];

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

    public void Dispose()
    {
        _initializationLock.Dispose();
    }
}