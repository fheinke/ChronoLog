namespace ChronoLog.SqlDatabase.Models;

public class ProjectEntity
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string ResponseObject { get; set; }
    public string? DefaultResponseText { get; set; }
}