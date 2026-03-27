using System.ComponentModel.DataAnnotations;

namespace ChronoLog.Core.Models.DTOs;

public record TimeEntryRequest(
    [Required] Guid WorkdayId,
    Guid? ProjectId,
    [Required] TimeSpan Duration,
    string? ResponseText
);

public record TimeEntryUpdateRequest(
    Guid? WorkdayId,
    Guid? ProjectId,
    TimeSpan? Duration,
    string? ResponseText
);