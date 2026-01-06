using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

    [Required] [MaxLength(500)] public string Roles { get; set; } = string.Empty;
    [Required] [Range(0, 365)] public int VacationDaysPerYear { get; set; }
    [Required] public double OvertimeHours { get; set; }

    [Required] public DateTime LastSeen { get; set; }

    public virtual ICollection<WorkdayEntity> Workdays { get; set; } = new List<WorkdayEntity>();

    [NotMapped]
    public List<string> RoleList =>
        Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    [NotMapped] public string DisplayName => Name ?? Email;

    [NotMapped] public bool IsAdmin => RoleList.Contains("Admin");
}