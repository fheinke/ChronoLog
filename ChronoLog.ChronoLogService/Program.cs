using ChronoLog.Applications;
using ChronoLog.ChronoLogService.Components;
using ChronoLog.ChronoLogService.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySql.Data.MySqlClient;
using ChronoLog.SqlDatabase;
using ChronoLog.SqlDatabase.Context;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Authentication & Authorization
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization(options => { options.FallbackPolicy = options.DefaultPolicy; });

// MVC & Razor
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters.Add(new ChronoLog.Applications.Converters.TimeOnlyJsonConverter());
    });
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Blazor
builder.Services.AddServerSideBlazor()
    .AddMicrosoftIdentityConsentHandler();

// Radzen
builder.Services.AddRadzenComponents();
builder.Services.AddRadzenCookieThemeService(options =>
{
    options.Name = "ChronoLogTheme";
    options.Duration = TimeSpan.FromDays(365);
});

// Database & Services
builder.Services.AddSqlServices();
builder.Services.AddApplicationsToServiceCollection(builder.Configuration);
builder.Services.AddHttpContextAccessor();

// API Documentation
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerExtension();
    builder.Services.AddSwaggerGen(options =>
    {
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
    });
}

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<SqlDbContext>("DbConnectionCheck");

// Logging
builder.Services.AddLogging(options =>
{
    options.AddSimpleConsole(opt => opt.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ");
});

// Header Progagation
builder.Services.AddHeaderPropagation(options => { options.Headers.Add("x-auth-request-access-token"); });

var app = builder.Build();

// Database initialization check
EnsureDatabaseConnection(app);

// Apply pending migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SqlDbContext>();
    await db.Database.MigrateAsync();
}

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseHeaderPropagation();
app.UseAntiforgery();

// Endpoints
app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Health Check Endpoint
app.MapHealthChecks("/.well-known/readiness", new HealthCheckOptions
{
    Predicate = check => check.Name == "DbConnectionCheck",
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    },
    AllowCachingResponses = false
});

app.Run();
return;

static void EnsureDatabaseConnection(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    try
    {
        var sqlDbContext = scope.ServiceProvider.GetRequiredService<SqlDbContext>();
        sqlDbContext.Database.GetService<IRelationalDatabaseCreator>();
        
        if (!sqlDbContext.Database.CanConnect())
        {
            app.Logger.LogCritical("Unable to connect to the database");
            throw new Exception("Unable to connect to the database");
        }
        
        app.Logger.LogInformation("Database connection established successfully");
    }
    catch (MySqlException ex)
    {
        app.Logger.LogCritical(ex, "Failed to connect to database. Application will terminate.");
        throw;
    }
}