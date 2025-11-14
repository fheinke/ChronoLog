using ChronoLog.SqlDatabase.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChronoLog.SqlDatabase;

public static class AddDbContext
{
    public static IServiceCollection AddSqlServices(this IServiceCollection serviceCollection)
    {
        var config = serviceCollection.BuildServiceProvider()
            .GetService<IConfiguration>();
        serviceCollection.AddDbContext<SqlDbContext>(
            x => x.UseMySQL(config!.GetConnectionString("SqlDatabase")!, y => y.CommandTimeout(120)),
            ServiceLifetime.Transient);
        return serviceCollection;
    }
}