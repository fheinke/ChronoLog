using System.ComponentModel.DataAnnotations;

namespace ChronoLog.Core.Models.DisplayObjects;

public class EmployeeModel
{
    public Guid EmployeeId { get; set; }

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
    [Required] public double DailyWorkingTimeInHours { get; set; }
    public double OvertimeCorrectionInHours { get; set; }
    
    [Required] public DateTime LastSeen { get; set; }
}