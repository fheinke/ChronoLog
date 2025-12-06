using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ChronoLog.Applications.Services;
using ChronoLog.Core.Interfaces;

namespace ChronoLog.Applications;

public static class AddApplications
{
    public static IServiceCollection AddApplicationsToServiceCollection(this IServiceCollection serviceCollection,
        IConfiguration config)
    {
        serviceCollection.AddScoped<ITimeService, TimeService>();
        serviceCollection.AddScoped<IProjectService, ProjectService>();
        
        return serviceCollection;
    }
}