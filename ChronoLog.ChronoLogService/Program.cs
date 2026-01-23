using System.Globalization;
using ChronoLog.Applications;
using ChronoLog.ChronoLogService.Authorization;
using ChronoLog.ChronoLogService.Components;
using ChronoLog.ChronoLogService.Extensions;
using ChronoLog.ChronoLogService.Services;
using ChronoLog.Core.Interfaces;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ChronoLog.SqlDatabase;
using ChronoLog.SqlDatabase.Context;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Radzen;
using static System.Int32;
using ApiUserService = ChronoLog.ChronoLogService.Authorization.ApiUserService;

var builder = WebApplication.CreateBuilder(args);

// Set default culture to de-DE
var cultureInfo = new CultureInfo("de-DE");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Authentication & Authorization
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "JWT_OR_COOKIE";
        options.DefaultChallengeScheme = "JWT_OR_COOKIE";
    })
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();

builder.Services.AddAuthentication()
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"), "Bearer");

builder.Services.AddAuthentication()
    .AddPolicyScheme("JWT_OR_COOKIE", "JWT_OR_COOKIE", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            string authorization = context.Request.Headers[Microsoft.Net.Http.Headers.HeaderNames.Authorization]!;
            if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
            {
                return "Bearer";
            }

            return OpenIdConnectDefaults.AuthenticationScheme;
        };
    });

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.Secure = CookieSecurePolicy.Always;
});

// Authorization Handlers
builder.Services.AddScoped<IApiUserService, ApiUserService>();
builder.Services.AddScoped<IAuthorizationHandler, ProjectManagementHandler>();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;

    options.AddPolicy("ProjectManagement", policy =>
        policy.Requirements.Add(new ProjectManagementRequirement()));
});

// MVC & Razor
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters.Add(new ChronoLog.Applications.Converters.TimeOnlyJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new ChronoLog.Applications.Converters.DateOnlyJsonConverter());
    })
    .ConfigureApiBehaviorOptions(options => { options.SuppressModelStateInvalidFilter = false; });
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
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserServiceFactory>();
builder.Services.AddScoped<IUserService>(sp =>
{
    var factory = sp.GetRequiredService<UserServiceFactory>();
    return factory.Create();
});
builder.Services.AddApplicationsToServiceCollection(builder.Configuration);

// API Documentation
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerExtension();
    builder.Services.AddSwaggerGen(options =>
    {
        options.DocInclusionPredicate((_, apiDesc) =>
            !apiDesc.RelativePath?.StartsWith("MicrosoftIdentity") ?? true);
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

        var knownProxies = builder.Configuration.GetValue<string>("ReverseProxy:KnownProxies");
        if (!string.IsNullOrWhiteSpace(knownProxies))
        {
            foreach (var proxy in knownProxies.Split(',',
                         StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                if (proxy.Contains('/'))
                {
                    var parts = proxy.Split('/');
                    if (parts.Length != 2) continue;
                    var prefixLength = TryParse(parts[1], out var length) ? length : 32;
                    options.KnownIPNetworks.Add(new System.Net.IPNetwork(System.Net.IPAddress.Parse(parts[0]),
                        prefixLength));
                }
                else if (System.Net.IPAddress.TryParse(proxy, out var ipAddress))
                {
                    options.KnownProxies.Add(ipAddress);
                }
            }
        }
        else
        {
            options.KnownIPNetworks.Add(new System.Net.IPNetwork(
                System.Net.IPAddress.Parse("10.0.0.0"), 8));
            options.KnownIPNetworks.Add(new System.Net.IPNetwork(
                System.Net.IPAddress.Parse("172.16.0.0"), 12));
            options.KnownIPNetworks.Add(new System.Net.IPNetwork(
                System.Net.IPAddress.Parse("192.168.0.0"), 16));
        }

        options.ForwardLimit = 1;
        options.RequireHeaderSymmetry = false;
    });

    var baseUrl = builder.Configuration.GetValue<string>("ReverseProxy:BaseUrl");
    builder.Services.PostConfigure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.NonceCookie.SameSite = SameSiteMode.None;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.CorrelationCookie.SameSite = SameSiteMode.None;

        var originalOnRedirectToIdentityProvider = options.Events.OnRedirectToIdentityProvider;
        options.Events.OnRedirectToIdentityProvider = async context =>
        {
            context.ProtocolMessage.RedirectUri = $"{baseUrl}/signin-oidc";
            await originalOnRedirectToIdentityProvider(context);
        };

        var originalOnRedirectToIdentityProviderForSignOut = options.Events.OnRedirectToIdentityProviderForSignOut;
        options.Events.OnRedirectToIdentityProviderForSignOut = async context =>
        {
            context.ProtocolMessage.PostLogoutRedirectUri = $"{baseUrl}/signout-callback-oidc";
            await originalOnRedirectToIdentityProviderForSignOut(context);
        };
    });
}

var app = builder.Build();

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

if (useReverseProxy)
    app.UseForwardedHeaders();

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
}).AllowAnonymous();

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

app.Run();