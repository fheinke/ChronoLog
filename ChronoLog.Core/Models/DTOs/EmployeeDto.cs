namespace ChronoLog.Core.Models.DTOs;

public sealed record EmployeeDto(
    Guid EmployeeId,
    string ObjectId,
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