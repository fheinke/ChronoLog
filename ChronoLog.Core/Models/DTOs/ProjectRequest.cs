using System.ComponentModel.DataAnnotations;

namespace ChronoLog.Core.Models.DTOs;

public record ProjectRequest(
    [Required] [MaxLength(256)] string Name,
    [MaxLength(1024)] string? Description,
    [Required] [MaxLength(256)] string ResponseObject,
    [MaxLength(1024)] string? DefaultResponseText,
    bool? IsDefault
);

public record ProjectUpdateRequest(
    [MaxLength(256)] string? Name,
    [MaxLength(1024)] string? Description,
    [MaxLength(256)] string? ResponseObject,
    [MaxLength(1024)] string? DefaultResponseText,
    bool? IsDefault
);