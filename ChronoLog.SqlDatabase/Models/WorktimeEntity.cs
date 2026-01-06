using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChronoLog.SqlDatabase.Models;

[CustomValidation(typeof(WorktimeEntity), nameof(ValidateEndTime))]
public class WorktimeEntity
{
    [Key] public Guid WorktimeId { get; set; }

    [Required] public Guid WorkdayId { get; set; }

    [ForeignKey(nameof(WorkdayId))] public WorkdayEntity Workday { get; set; } = null!;

    [Required] public TimeOnly StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public TimeSpan? BreakTime { get; set; }

    public static ValidationResult? ValidateEndTime(WorktimeEntity entity, ValidationContext context)
    {
        if (entity.EndTime.HasValue && entity.EndTime < entity.StartTime)
            return new ValidationResult("EndTime cannot be earlier than StartTime.");
        return ValidationResult.Success;
    }
}