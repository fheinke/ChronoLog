namespace ChronoLog.Core.Models.DisplayObjects;

public class EmployeeModel
{
    public Guid EmployeeId { get; set; }
    
    public string ObjectId { get; set; } // Unique identifier from identity provider
    public string Email { get; set; }
    public string? Name { get; set; }
    
    public GermanProvince Province { get; set; }
    public string Roles { get; set; }
    public int VacationDaysPerYear { get; set; }
    public double OvertimeHours { get; set; }
}