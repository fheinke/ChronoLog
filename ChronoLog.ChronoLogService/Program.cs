using ChronoLog.Applications;
using ChronoLog.ChronoLogService.Components;
using ChronoLog.ChronoLogService.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySql.Data.MySqlClient;
using ChronoLog.SqlDatabase;
using ChronoLog.SqlDatabase.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSqlServices();
builder.Services.AddHttpContextAccessor();
builder.Services.AddApplicationsToServiceCollection(builder.Configuration);
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters.Add(new ChronoLog.Applications.Converters.TimeOnlyJsonConverter());
    });
builder.Services.AddSwaggerExtension();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<SqlDbContext>("DbConnectionCheck");
builder.Services.AddLogging(options =>
{
    options.AddSimpleConsole(opt => opt.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ");
});

builder.Services.AddHeaderPropagation(o => { o.Headers.Add("x-auth-request-access-token"); });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var scopedProvider = scope.ServiceProvider;

    try
    {
        var sqlDbContext = scopedProvider.GetRequiredService<SqlDbContext>();
        sqlDbContext.Database.CanConnect();
        sqlDbContext.Database.GetService <IRelationalDatabaseCreator>()
            .HasTables();
    }
    catch (MySqlException e)
    {
        app.Logger.LogError(e, "Connection to database failed.");
        throw;
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseHeaderPropagation();
app.UseAntiforgery();
app.MapStaticAssets();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.UseHealthChecks("/.well-known/readiness", new HealthCheckOptions
{
    Predicate = (check) => check.Name == "DbConnectionCheck",
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    },
    AllowCachingResponses = false
});

app.Run();