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
        serviceCollection.AddScoped<IProjectService, ProjectService>();
        serviceCollection.AddScoped<IProjecttimeService, ProjecttimeService>();
        serviceCollection.AddScoped<IUserService, UserService>();
        serviceCollection.AddScoped<IWorkdayService, WorkdayService>();
        serviceCollection.AddScoped<IWorktimeService, WorktimeService>();
        
        return serviceCollection;
    }
}