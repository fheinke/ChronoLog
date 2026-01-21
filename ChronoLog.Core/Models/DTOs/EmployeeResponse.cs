namespace ChronoLog.Core.Models.DTOs;

public record EmployeeResponse(
    Guid EmployeeId,
    string Email,
    string? Name,
    GermanProvince Province,
    bool? IsAdmin,
    bool? IsProjectManager,
    int VacationDaysPerYear,
    double DailyWorkingTimeInHours,
    double OvertimeCorrectionInHours,
    DateTime LastSeen
);