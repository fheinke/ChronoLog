using System.ComponentModel.DataAnnotations;
using ChronoLog.Core.Models;

namespace ChronoLog.SqlDatabase.Models;

public class EmployeeEntity
{
    [Key] public Guid EmployeeId { get; set; }

    [Required] [MaxLength(256)] public string ObjectId { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(128)] public string? Name { get; set; }

    [Required] public GermanProvince Province { get; set; }

    public bool? IsAdmin { get; set; }
    public bool? IsProjectManager { get; set; }
    [Required] [Range(0, 365)] public int VacationDaysPerYear { get; set; }
    [Required] [Range(0.0, 24.0)] public double DailyWorkingTimeInHours { get; set; }
    public double OvertimeCorrectionInHours { get; set; }

    /// <summary>Comma separated list of <see cref="DayOfWeek"/> values (0 = Sunday) that are regular working days.</summary>
    [Required] [MaxLength(32)] public string WorkingDays { get; set; } = "1,2,3,4,5";

    [Required] public DateTime LastSeen { get; set; }

    public virtual ICollection<WorkdayEntity> Workdays { get; set; } = new List<WorkdayEntity>();
}