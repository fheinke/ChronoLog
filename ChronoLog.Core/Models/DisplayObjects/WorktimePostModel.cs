using System.ComponentModel.DataAnnotations;

namespace ChronoLog.Core.Models.DisplayObjects;

public class WorktimePostModel
{
    public Guid WorkdayId { get; set; }
    [Required]
    [DataType(DataType.Time)]
    public TimeOnly StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public TimeSpan? BreakTime { get; set; }
}