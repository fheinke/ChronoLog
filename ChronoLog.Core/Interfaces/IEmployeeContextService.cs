using ChronoLog.Core.Models.DisplayObjects;

namespace ChronoLog.Core.Interfaces;

public interface IEmployeeContextService
{
    Task<EmployeeModel> GetOrCreateCurrentEmployeeAsync();
    Task<List<AbsenceEntryModel>> GetEmployeeAbsenceDaysAsync(Guid employeeId, int year);
}