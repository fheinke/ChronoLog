using ChronoLog.SqlDatabase.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChronoLog.SqlDatabase;

public static class AddDbContext
{
    public static void AddSqlServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContextFactory<SqlDbContext>(options =>
            options.UseMySQL(
                configuration.GetConnectionString("SqlDatabase")
                ?? throw new InvalidOperationException("Connection string 'SqlDatabase' not found."),
                mysqlOptions => mysqlOptions.CommandTimeout(120)
            )
        );
    }
}