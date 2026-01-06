using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChronoLog.SqlDatabase.Models;

public class ProjecttimeEntity
{
    [Key] public Guid ProjecttimeId { get; set; }

    [Required] public Guid WorkdayId { get; set; }
    [ForeignKey(nameof(WorkdayId))] public WorkdayEntity Workday { get; set; } = null!;

    [Required] public Guid ProjectId { get; set; }
    [ForeignKey(nameof(ProjectId))] public ProjectEntity Project { get; set; } = null!;

    [Required] public TimeSpan TimeSpent { get; set; }

    [MaxLength(1024)] public string? ResponseText { get; set; }
}