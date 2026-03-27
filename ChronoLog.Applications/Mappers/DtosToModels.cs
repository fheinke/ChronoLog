using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.Core.Models.DTOs;

namespace ChronoLog.Applications.Mappers;

public static class DtosToModels
{
    public static EmployeeModel ToModel(this EmployeeDto dto)
    {
        return new EmployeeModel
        {
            EmployeeId = dto.EmployeeId,
            ObjectId = dto.ObjectId,
            Email = dto.Email,
            Name = dto.Name,
            Province = dto.Province,
            IsAdmin = dto.IsAdmin,
            IsProjectManager = dto.IsProjectManager,
            VacationDaysPerYear = dto.VacationDaysPerYear,
            DailyWorkingTimeInHours = dto.DailyWorkingTimeInHours,
            OvertimeCorrectionInHours = dto.OvertimeCorrectionInHours,
            LastSeen = dto.LastSeen
        };
    }
}