using ChronoLog.Applications.Mappers;
using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models;
using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.SqlDatabase.Context;
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

    public async Task<EmployeeModel?> GetCurrentEmployeeAsync()
    {
        if (_cachedEmployee is not null)
            return _cachedEmployee;

        var userId = await _userService.GetUserIdAsync();
        if (string.IsNullOrEmpty(userId))
            return null;
        
        _cachedEmployee = await _sqlDbContext.Employees
            .AsNoTracking()
            .Select(e => e.ObjectId == userId ? e.ToModel() : null)
            .FirstOrDefaultAsync();
        
        return _cachedEmployee;
    }

    public async Task<EmployeeModel> GetOrCreateCurrentEmployeeAsync()
    {
        var employee = await GetCurrentEmployeeAsync();
        if (employee != null)
            return employee;

        var userId = await _userService.GetUserIdAsync();
        var email = await _userService.GetUserEmailAsync();
        var name = await _userService.GetUserNameAsync();

        var newEmployee = new EmployeeModel
        {
            EmployeeId = Guid.NewGuid(),
            ObjectId = userId!,
            Email = email!,
            Name = name ?? "",
            Province = GermanProvince.ALL,
            Roles = "",
            VacationDaysPerYear = 30,
            OvertimeHours = 0
        };

        await _sqlDbContext.Employees.AddAsync(newEmployee.ToEntity());
        var affectedRows = await _sqlDbContext.SaveChangesAsync();

        if (affectedRows == 0)
            throw new Exception("Failed to create new employee.");
        
        _cachedEmployee = newEmployee;
        return newEmployee;
    }
}