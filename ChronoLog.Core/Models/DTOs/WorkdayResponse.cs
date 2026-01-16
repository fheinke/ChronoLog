using ChronoLog.Core.Models.DisplayObjects;

namespace ChronoLog.Core.Models.DTOs;

public record WorkdayResponse(
    Guid WorkdayId,
    Guid EmployeeId,
    DateTime Date,
    WorkdayType Type,
    List<WorktimeModel> Worktimes,
    List<ProjecttimeModel> Projecttimes
);

public record WorkdayTypesResponse(
    List<WorkdayType> Types,
    List<WorkdayType> WorkingDays,
    List<WorkdayType> NonWorkingDays
);