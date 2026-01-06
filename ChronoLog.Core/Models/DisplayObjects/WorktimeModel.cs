using System.ComponentModel.DataAnnotations;

namespace ChronoLog.Core.Models.DisplayObjects;

public class WorktimeModel
{
    public Guid WorktimeId { get; set; }
    [Required] public Guid WorkdayId { get; set; }
    [Required] public TimeOnly StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public TimeSpan? BreakTime { get; set; }
}