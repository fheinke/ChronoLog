using ChronoLog.Core.Models.DisplayObjects;

namespace ChronoLog.Core.Models.DTOs;

public record WorkdayResponse(
    Guid WorkdayId,
    Guid EmployeeId,
    DateTime Date,
    WorkdayType Type,
    List<WorktimeModel> Worktimes,
    List<TimeEntryModel> TimeEntries
);

public record WorkdaySummaryResponse(
    Guid WorkdayId,
    DateOnly Date,
    TimeSpan TotalWorktime,
    WorkdayType Type,
    double OvertimeInHours
);

public record WorkdayTypesResponse(
    List<WorkdayType> Types,
    List<WorkdayType> WorkingDays,
    List<WorkdayType> NonWorkingDays
);