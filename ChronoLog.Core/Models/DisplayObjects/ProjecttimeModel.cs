using System.ComponentModel.DataAnnotations;

namespace ChronoLog.Core.Models.DisplayObjects;

public class ProjecttimeModel
{
    public Guid ProjecttimeId { get; set; }
    [Required] public Guid WorkdayId { get; set; }
    [Required] public Guid ProjectId { get; set; }
    [Required] public TimeSpan TimeSpent { get; set; }
    [MaxLength(1024)] public string? ResponseText { get; set; }
}