namespace ChronoLog.Core.Models.DisplayObjects;

public class WorkdayModel
{
    public Guid WorkdayId { get; set; }
    public DateOnly Date { get; set; }
    public WorkdayType Type { get; set; }
}