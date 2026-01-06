using System.ComponentModel.DataAnnotations;

namespace ChronoLog.Core.Models.DisplayObjects;

public class ProjectModel
{
    public Guid ProjectId { get; set; }

    [Required] public bool IsDefault { get; set; }
    [Required] [MaxLength(256)] public string Name { get; set; } = string.Empty;
    [MaxLength(1024)] public string? Description { get; set; }
    [Required] [MaxLength(256)] public string ResponseObject { get; set; } = string.Empty;
    [MaxLength(1024)] public string? DefaultResponseText { get; set; }
}