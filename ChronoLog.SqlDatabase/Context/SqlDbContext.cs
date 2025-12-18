using Microsoft.EntityFrameworkCore;
using ChronoLog.SqlDatabase.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ChronoLog.SqlDatabase.Context;

public class SqlDbContext(DbContextOptions<SqlDbContext> options) : DbContext(options)
{
    public DbSet<WorkdayEntity> Workdays { get; set; }
    public DbSet<WorktimeEntity> Worktimes { get; set; }
    public DbSet<ProjecttimeEntity> Projecttimes { get; set; }
    public DbSet<ProjectEntity> Projects { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
            d => d.ToDateTime(TimeOnly.MinValue),
            dt => DateOnly.FromDateTime(dt));
        
        var timeOnlyConverter = new ValueConverter<TimeOnly, TimeSpan>(
            t => t.ToTimeSpan(),
            ts => TimeOnly.FromTimeSpan(ts));

        var nullableTimeOnlyConverter = new ValueConverter<TimeOnly?, TimeSpan?>(
            t => t.HasValue ? t.Value.ToTimeSpan() : null,
            ts => ts.HasValue ? TimeOnly.FromTimeSpan(ts.Value) : (TimeOnly?)null);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            
            var dateProperties = clrType.GetProperties()
                .Where(p => p.PropertyType == typeof(DateOnly));
            foreach (var property in dateProperties)
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(property.PropertyType, property.Name)
                    .HasConversion(dateOnlyConverter);
            }
            
            var timeProperties = clrType.GetProperties()
                .Where(p => p.PropertyType == typeof(TimeOnly) || p.PropertyType == typeof(TimeOnly?));
            foreach (var property in timeProperties)
            {
                var propBuilder = modelBuilder.Entity(entityType.ClrType)
                    .Property(property.PropertyType, property.Name)
                    .HasColumnType("time");

                if (property.PropertyType == typeof(TimeOnly))
                    propBuilder.HasConversion(timeOnlyConverter);
                else
                    propBuilder.HasConversion(nullableTimeOnlyConverter);
            }
        }
        
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
            .HasMany<ProjecttimeEntity>(x => x.Projecttimes)
            .WithOne(x => x.Project)
            .HasForeignKey(x => x.ProjectId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
}