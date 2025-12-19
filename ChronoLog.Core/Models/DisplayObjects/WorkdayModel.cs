namespace ChronoLog.Core.Models.DisplayObjects;

public class WorkdayModel
{
    public Guid WorkdayId { get; set; }
    public DateTime Date { get; set; }
    public WorkdayType Type { get; set; }
}