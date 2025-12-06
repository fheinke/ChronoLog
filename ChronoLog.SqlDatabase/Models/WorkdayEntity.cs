using ChronoLog.Core.Models;

namespace ChronoLog.SqlDatabase.Models;

public class WorkdayEntity
{
    public Guid WorkdayId { get; set; }
    
    public virtual ICollection<WorktimeEntity> Worktimes { get; set; }
    public virtual ICollection<ProjecttimeEntity> Projecttimes { get; set; }
    
    public DateOnly Date { get; set; }
    public WorkdayType Type { get; set; }
}