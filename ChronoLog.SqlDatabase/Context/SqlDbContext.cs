using ChronoLog.SqlDatabase.Models;
using Microsoft.EntityFrameworkCore;

namespace ChronoLog.SqlDatabase.Context;

public class SqlDbContext(DbContextOptions<SqlDbContext> options) : DbContext(options)
{
    public DbSet<WorkdayEntity> Workdays { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkdayEntity>()
            .HasKey(x => x.WorkdayId);
    }
}