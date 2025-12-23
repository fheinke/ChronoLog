namespace ChronoLog.SqlDatabase.Models;

public class ProjecttimeEntity
{
    public Guid ProjecttimeId { get; set; }
    
    public Guid WorkdayId { get; set; }
    public WorkdayEntity Workday { get; set; }
    public Guid ProjectId { get; set; }
    public ProjectEntity Project { get; set; }
    
    public TimeSpan TimeSpent { get; set; }
    public string? ResponseText { get; set; }
}