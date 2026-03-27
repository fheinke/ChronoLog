using ChronoLog.Core.Models.DTOs;
using ChronoLog.Core.Models.HelperObjects;

namespace ChronoLog.Core.Interfaces;

public interface IEmployeeContextService
{
    Task<EmployeeDto> GetOrCreateCurrentEmployeeAsync();
    Task<List<AbsenceEntryModel>> GetEmployeeAbsenceDaysAsync(Guid employeeId, int year);
    Task<int> GetEmployeeVacationDaysCountAsync(Guid employeeId, int year);
    Task<List<EmployeeDto>> GetAllEmployeesAsync();
    Task<bool> UpdateEmployeeAsync(EmployeeDto employee);
}