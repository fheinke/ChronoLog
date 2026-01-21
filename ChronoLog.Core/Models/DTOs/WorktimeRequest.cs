using System.ComponentModel.DataAnnotations;

namespace ChronoLog.Core.Models.DTOs;

public record WorktimeRequest(
    [Required] Guid WorkdayId,
    [Required] [DataType(DataType.Time)] TimeOnly StartTime,
    TimeOnly? EndTime,
    TimeSpan? BreakTime
);

public record WorktimeUpdateRequest(
    [DataType(DataType.Time)] TimeOnly? StartTime,
    TimeOnly? EndTime,
    TimeSpan? BreakTime
);