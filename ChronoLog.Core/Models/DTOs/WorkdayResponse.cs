namespace ChronoLog.Core.Models.DTOs;

public record WorkdayTypesResponse(
    List<WorkdayType> Types,
    List<WorkdayType> WorkingDays,
    List<WorkdayType> NonWorkingDays
);