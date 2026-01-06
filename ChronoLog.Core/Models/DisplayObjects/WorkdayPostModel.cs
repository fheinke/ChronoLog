namespace ChronoLog.Core.Models.DisplayObjects;

public class WorkdayPostModel
{
    public Guid EmployeeId { get; set; }
    public DateTime? Date { get; set; }
    public WorkdayType Type { get; set; }
}