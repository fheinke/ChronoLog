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
        // Workday
        modelBuilder.Entity<WorkdayEntity>()
            .HasKey(x => x.WorkdayId);
        modelBuilder.Entity<WorkdayEntity>()
            .HasMany<WorktimeEntity>(x => x.Worktimes)
            .WithOne(x => x.Workday)
            .HasForeignKey(x => x.WorkdayId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Worktime
        modelBuilder.Entity<WorktimeEntity>()
            .HasKey(x => x.WorktimeId);
        
        // Projecttime
        modelBuilder.Entity<ProjecttimeEntity>()
            .HasKey(x => x.ProjecttimeId);
        
        // Project
        modelBuilder.Entity<ProjectEntity>()
            .HasKey(x => x.ProjectId);
        modelBuilder.Entity<ProjectEntity>()
            .HasIndex(x => x.IsDefault)
            .IsUnique()
            .HasFilter("IsDefault = 1");
        modelBuilder.Entity<ProjectEntity>()
            .HasMany<ProjecttimeEntity>(x => x.Projecttimes)
            .WithOne(x => x.Project)
            .HasForeignKey(x => x.ProjectId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
}