using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.Core.Models.DTOs;

namespace ChronoLog.Applications.Mappers;

public static class ModelsToDtos
{
    public static EmployeeDto ToDto(this EmployeeModel model)
    {
        return new EmployeeDto(
            model.EmployeeId,
            model.ObjectId,
            model.Email,
            model.Name,
            model.Province,
            model.IsAdmin,
            model.IsProjectManager,
            model.VacationDaysPerYear,
            model.DailyWorkingTimeInHours,
            model.OvertimeCorrectionInHours,
            model.LastSeen
        );
    }
}