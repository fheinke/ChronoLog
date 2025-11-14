using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;

namespace ChronoLog.ChronoLogService.Extensions;

public static class SwaggerExtension
{
    public static IServiceCollection AddSwaggerExtension(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(swaggerGenOptions =>
        {
            swaggerGenOptions.SwaggerDoc(
                "v1", 
                new OpenApiInfo
                {
                    Version = "v1",
                    Title = "ChronoLog API",
                    Description = "ChronoLog API",
                    Contact = new OpenApiContact
                    {
                        Name = "Felix Heinke"
                    },
                    License = new OpenApiLicense { Name = "Propritary" }
                });

            var filePath = Path.Combine(
                AppContext.BaseDirectory,
                "ChronoLog.ChronoLogService.xml");
            swaggerGenOptions.IncludeXmlComments(filePath);
        });
        return services;
    }
}