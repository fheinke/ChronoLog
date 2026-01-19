namespace ChronoLog.Core.Models.DTOs;

public record EmployeeUpdateRequest(
    GermanProvince? Province,
    int? VacationDaysPerYear,
    double? DailyWorkingTimeInHours,
    double? OvertimeCorrectionInHours
);