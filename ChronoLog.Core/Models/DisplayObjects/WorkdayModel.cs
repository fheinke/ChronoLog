using System.ComponentModel.DataAnnotations;

namespace ChronoLog.Core.Models.DisplayObjects;

public class WorkdayModel
{
    public Guid WorkdayId { get; set; }
    [Required]
    public Guid EmployeeId { get; set; }
    [Required]
    public DateTime Date { get; set; }
    [Required]
    public WorkdayType Type { get; set; }
}