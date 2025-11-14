namespace ChronoLog.SqlDatabase.Models;

public class WorkdayEntity
{
    public Guid WorkdayId { get; set; }
    public DateOnly Date { get; set; }
}