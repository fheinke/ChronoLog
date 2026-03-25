using ChronoLog.Core.Models.DTOs;
using ChronoLog.SqlDatabase.Models;

namespace ChronoLog.Applications.Mappers;

public static class EntitiesToDtos
{
    public static EmployeeDto ToDto(this EmployeeEntity entity)
    {
        return new EmployeeDto(
            entity.EmployeeId,
            entity.ObjectId,
            entity.Email,
            entity.Name,
            entity.Province,
            entity.IsAdmin,
            entity.IsProjectManager,
            entity.VacationDaysPerYear,
            entity.DailyWorkingTimeInHours,
            entity.OvertimeCorrectionInHours,
            entity.LastSeen
        );
    }
}