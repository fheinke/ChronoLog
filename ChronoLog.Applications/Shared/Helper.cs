using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models.DTOs;

namespace ChronoLog.Applications.Shared;

public static class Helper
{
    public static async Task<EmployeeDto> GetCurrentEmployeeAsync(IEmployeeContextService employeeContextService)
    {
        var employee = await employeeContextService.GetOrCreateCurrentEmployeeAsync();
        return employee;
    }
    
    public static async Task<Guid> GetCurrentEmployeeIdAsync(IEmployeeContextService employeeContextService)
    {
        var employee = await employeeContextService.GetOrCreateCurrentEmployeeAsync();
        return employee.EmployeeId;
    }
}