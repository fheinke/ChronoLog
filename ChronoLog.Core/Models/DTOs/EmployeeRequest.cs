namespace ChronoLog.Core.Models.DTOs;

public record EmployeeUpdateRequest(
    GermanProvince? Province,
    int? VacationDaysPerYear,
    double? DailyWorkingTimeInHours,
    double? OvertimeCorrectionInHours,
    List<DayOfWeek>? WorkingDays = null
);