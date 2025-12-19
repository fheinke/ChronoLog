namespace ChronoLog.Core.Models.DisplayObjects;

public class WorkdayViewModel
{
    public Guid WorkdayId { get; set; }
    public DateTime Date { get; set; }
    public WorkdayType Type { get; set; }
    public List<WorktimeModel> Worktimes { get; set; }
    public List<ProjecttimeModel> Projecttimes { get; set; }
    
    public string TypeText => Type.ToString();
}