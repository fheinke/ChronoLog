using Microsoft.Extensions.DependencyInjection;
using ChronoLog.Applications.Services;
using ChronoLog.Core.Interfaces;

namespace ChronoLog.Applications;

public static class AddApplications
{
    public static void AddApplicationsToServiceCollection(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeContextService, EmployeeContextService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITimeEntryService, TimeEntryService>();
        services.AddScoped<IWorkdayService, WorkdayService>();
        services.AddScoped<IWorktimeService, WorktimeService>();
    }
}