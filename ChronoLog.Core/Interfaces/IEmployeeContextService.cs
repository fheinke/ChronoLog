using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.Core.Models.HelperObjects;

namespace ChronoLog.Core.Interfaces;

public interface IEmployeeContextService
{
    Task<EmployeeModel> GetOrCreateCurrentEmployeeAsync();
    Task<List<AbsenceEntryModel>> GetEmployeeAbsenceDaysAsync(Guid employeeId, int year);
    Task<int> GetEmployeeVacationDaysCountAsync(Guid employeeId, int year);
    Task<List<EmployeeModel>> GetAllEmployeesAsync();
    Task<bool> UpdateEmployeeAsync(EmployeeModel employee);
}