using System.ComponentModel.DataAnnotations;

namespace ChronoLog.Core.Models.DisplayObjects;

public class TimeEntryModel
{
    public Guid TimeEntryId { get; set; }
    [Required] public Guid WorkdayId { get; set; }
    [Required] public Guid ProjectId { get; set; }
    [Required] public TimeSpan Duration { get; set; }
    [MaxLength(1024)] public string? ResponseText { get; set; }
}