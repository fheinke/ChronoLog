using Microsoft.EntityFrameworkCore;
using ChronoLog.SqlDatabase.Models;

namespace ChronoLog.SqlDatabase.Context;

public class SqlDbContext(DbContextOptions<SqlDbContext> options) : DbContext(options)
{
    public DbSet<WorkdayEntity> Workdays { get; set; }
    public DbSet<WorktimeEntity> Worktimes { get; set; }
    public DbSet<ProjecttimeEntity> Projecttimes { get; set; }
    public DbSet<ProjectEntity> Projects { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkdayEntity>()
            .HasKey(x => x.WorkdayId);
        
        modelBuilder.Entity<WorktimeEntity>()
            .HasKey(x => x.WorktimeId);
        
        modelBuilder.Entity<ProjecttimeEntity>()
            .HasKey(x => x.ProjecttimeId);
        
        modelBuilder.Entity<ProjectEntity>()
            .HasKey(x => x.ProjectId);
        modelBuilder.Entity<ProjectEntity>()
            .HasIndex(x => x.IsDefault)
            .IsUnique()
            .HasFilter("IsDefault = 1");
    }
}