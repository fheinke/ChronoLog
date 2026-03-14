using System.ComponentModel.DataAnnotations;

namespace ChronoLog.SqlDatabase.Models;

public class ProjectEntity
{
    [Key]
    public Guid ProjectId { get; set; }
    
    public virtual ICollection<TimeEntryEntity> TimeEntries { get; set; } = new List<TimeEntryEntity>();
    
    [Required]
    public bool IsDefault { get; set; }
    
    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1024)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(256)]
    public string ResponseObject { get; set; } = string.Empty;
    
    [MaxLength(1024)]
    public string? DefaultResponseText { get; set; }
}