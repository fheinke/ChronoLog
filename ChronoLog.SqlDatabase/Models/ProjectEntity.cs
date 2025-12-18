namespace ChronoLog.SqlDatabase.Models;

public class ProjectEntity
{
    public Guid ProjectId { get; set; }
    
    public virtual ICollection<ProjecttimeEntity> Projecttimes { get; set; }
    
    public bool IsDefault { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string ResponseObject { get; set; }
    public string? DefaultResponseText { get; set; }
}