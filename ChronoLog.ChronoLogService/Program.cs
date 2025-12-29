using ChronoLog.Applications;
using ChronoLog.ChronoLogService.Components;
using ChronoLog.ChronoLogService.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySql.Data.MySqlClient;
using ChronoLog.SqlDatabase;
using ChronoLog.SqlDatabase.Context;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Authentication
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration. GetSection("AzureAd"));
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
    .AddMicrosoftIdentityConsentHandler();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddRadzenComponents();
builder.Services.AddRadzenCookieThemeService(options =>
{
    options.Name = "ChronoLogTheme";
    options.Duration = TimeSpan.FromDays(365);
});
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
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

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
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseHeaderPropagation();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapFallbackToFile("index.html");

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