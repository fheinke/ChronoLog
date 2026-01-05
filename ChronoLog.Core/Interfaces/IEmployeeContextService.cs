using ChronoLog.Core.Models.DisplayObjects;

namespace ChronoLog.Core.Interfaces;

public interface IEmployeeContextService
{
    Task<EmployeeModel?> GetCurrentEmployeeAsync();
    Task<EmployeeModel> GetOrCreateCurrentEmployeeAsync();
}