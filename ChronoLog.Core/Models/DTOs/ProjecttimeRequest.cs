using System.ComponentModel.DataAnnotations;

namespace ChronoLog.Core.Models.DTOs;

public record ProjecttimeRequest(
    [Required] Guid WorkdayId,
    Guid? ProjectId,
    [Required] TimeSpan TimeSpent,
    string? ResponseText
);

public record ProjecttimeUpdateRequest(
    Guid? WorkdayId,
    Guid? ProjectId,
    TimeSpan? TimeSpent,
    string? ResponseText
);