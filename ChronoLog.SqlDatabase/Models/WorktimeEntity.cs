namespace ChronoLog.SqlDatabase.Models;

public class WorktimeEntity
{
    public Guid WorktimeId { get; set; }
    public Guid WorkdayId { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public TimeSpan BreakTime { get; set; }
}