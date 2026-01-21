using System.ComponentModel.DataAnnotations;

namespace ChronoLog.Core.Models.DTOs;

public record WorkdayRequest(
    [Required] DateTime Date,
    [Required] WorkdayType Type
);

public record WorkdayUpdateRequest(
    DateOnly? Date,
    WorkdayType? Type
);