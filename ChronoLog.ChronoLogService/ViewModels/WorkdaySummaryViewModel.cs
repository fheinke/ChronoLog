using ChronoLog.Core.Models;
using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.Core.Models.DTOs;

namespace ChronoLog.ChronoLogService.ViewModels;

public sealed class WorkdaySummaryViewModel
{
    public Guid WorkdayId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public WorkdayType Type { get; set; }
    public List<WorktimeModel> Worktimes { get; set; } = [];
    public List<TimeEntryModel> TimeEntries { get; set; } = [];
    
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
    
    public static WorkdaySummaryViewModel FromResponse(WorkdayResponse r) => new()
    {
        WorkdayId = r.WorkdayId,
        EmployeeId = r.EmployeeId,
        Date = r.Date,
        Type = r.Type,
        Worktimes = r.Worktimes,
        TimeEntries = r.TimeEntries
    };
}