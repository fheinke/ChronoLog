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

        var userId = await _userService.GetUserIdAsync();
        if (string.IsNullOrEmpty(userId))
            return null;

        var employee = await _sqlDbContext.Employees
            .FirstOrDefaultAsync(e => e.ObjectId == userId);

        if (employee is null)
            return null;

        employee.LastSeen = DateTime.UtcNow;
        var affectedRows = await _sqlDbContext.SaveChangesAsync();
        if (affectedRows == 0)
            throw new Exception("Failed to update employee's last seen timestamp.");

        _cachedEmployee = employee.ToModel();
        return _cachedEmployee;
    }

    public async Task<EmployeeModel> GetOrCreateCurrentEmployeeAsync()
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
            Roles = "",
            VacationDaysPerYear = 30,
            OvertimeHours = 0,
            LastSeen = DateTime.UtcNow,
        };
        
        await _sqlDbContext.Employees.AddAsync(newEmployee);
        var affectedRows = await _sqlDbContext.SaveChangesAsync();

        if (affectedRows == 0)
            throw new Exception("Failed to create new employee.");

        _cachedEmployee = newEmployee.ToModel();
        return _cachedEmployee;
    }
}