namespace ChronoLog.Core.Models.DisplayObjects;

public class WorkdayViewModel
{
    public Guid WorkdayId { get; set; }
    public DateTime Date { get; set; }
    public WorkdayType Type { get; set; }
    public List<WorktimeModel> Worktimes { get; set; }
    public List<ProjecttimeModel> Projecttimes { get; set; }
    
    public string TypeText => Type.ToString();
    
    // Helper properties for graphical representation of Workdays with Timespans
    public TimeOnly StartTimeOnly => Worktimes.Count != 0
        ? Worktimes.Min(wt => wt.StartTime)
        : TimeOnly.MinValue;
    public TimeOnly EndTimeOnly => Worktimes.Count != 0
        ? Worktimes.Max(wt => wt.EndTime ?? TimeOnly.MaxValue)
        : TimeOnly.MaxValue;
    public DateTime Start => Date.Date.Add(StartTimeOnly.ToTimeSpan());
    public DateTime End => Date.Date.Add(EndTimeOnly.ToTimeSpan());
    
    // Total worktime calculation with Breaktime consideration
    public TimeSpan TotalWorktime
    {
        get
        {
            var total = TimeSpan.Zero;
            foreach (var worktime in Worktimes)
            {
                if (worktime.EndTime.HasValue)
                {
                    var duration = worktime.EndTime.Value - worktime.StartTime;
                    total += duration;
                }

                if (worktime.BreakTime.HasValue)
                {
                    total -= worktime.BreakTime.Value;
                }
            }
            return total;
        }
    }
}