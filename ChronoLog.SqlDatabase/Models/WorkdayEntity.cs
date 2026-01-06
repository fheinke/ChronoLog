using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ChronoLog.Core.Models;

namespace ChronoLog.SqlDatabase.Models;

public class WorkdayEntity
{
    [Key] public Guid WorkdayId { get; set; }

    [Required] public Guid EmployeeId { get; set; }

    [ForeignKey(nameof(EmployeeId))] public EmployeeEntity Employee { get; set; } = null!;

    public virtual ICollection<WorktimeEntity> Worktimes { get; set; } = new List<WorktimeEntity>();
    public virtual ICollection<ProjecttimeEntity> Projecttimes { get; set; } = new List<ProjecttimeEntity>();

    [Required] public DateTime Date { get; set; }
    [Required] public WorkdayType Type { get; set; }
}