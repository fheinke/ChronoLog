using ChronoLog.Applications;
using ChronoLog.ChronoLogService.Components;
using ChronoLog.ChronoLogService.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ChronoLog.SqlDatabase;
using ChronoLog.SqlDatabase.Context;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
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

var useReverseProxy = builder.Configuration.GetValue<bool>("ReverseProxy:Enabled");
if (useReverseProxy)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                   ForwardedHeaders.XForwardedProto |
                                   ForwardedHeaders.XForwardedHost;

        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
        options.KnownProxies.Add(System.Net.IPAddress.Parse("127.0.0.1"));

        options.ForwardLimit = 1;
        options.RequireHeaderSymmetry = false;
    });

    var baseUrl = builder.Configuration.GetValue<string>("ReverseProxy:BaseUrl");
    builder.Services.PostConfigure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProvider = context =>
            {
                context.ProtocolMessage.RedirectUri = $"{baseUrl}/signin-oidc";
                return Task.CompletedTask;
            },
            OnRedirectToIdentityProviderForSignOut = context =>
            {
                context.ProtocolMessage.PostLogoutRedirectUri = $"{baseUrl}/signout-callback-oidc";
                return Task.CompletedTask;
            }
        };
    });
}

var app = builder.Build();

// Configure Forwarded Headers Middleware
if (useReverseProxy)
    app.UseForwardedHeaders();

// Apply pending migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SqlDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Checking for pending migrations...");
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "An error occurred while applying database migrations. Application will terminate.");
        throw;
    }
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
if (!useReverseProxy)
    app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseHeaderPropagation();
app.UseAntiforgery();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

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