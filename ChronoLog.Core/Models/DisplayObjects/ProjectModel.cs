namespace ChronoLog.Core.Models.DisplayObjects;

public class ProjectModel
{
    public Guid ProjectId { get; set; }
    public bool IsDefault { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string ResponseObject { get; set; }
    public string? DefaultResponseText { get; set; }
}