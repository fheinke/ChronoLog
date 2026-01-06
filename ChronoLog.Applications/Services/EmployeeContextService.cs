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

            _cachedEmployee = employee.ToModel();
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
            var existingEmployee = await GetCurrentEmployeeAsync();
            if (existingEmployee is not null)
                return existingEmployee;

            var userId = await _userService.GetUserIdAsync();

            var newEmployee = new EmployeeEntity
            {
                EmployeeId = Guid.NewGuid(),
                ObjectId = userId!,
                Email = await _userService.GetUserEmailAsync() ?? "",
                Name = await _userService.GetUserNameAsync() ?? "",
                Province = GermanProvince.ALL,
                Roles = "",
                VacationDaysPerYear = 30,
                OvertimeHours = 0,
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

    public void Dispose()
    {
        _initializationLock.Dispose();
    }
}