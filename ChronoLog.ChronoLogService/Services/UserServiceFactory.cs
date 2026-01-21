using ChronoLog.Applications.Services;
using ChronoLog.Core.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;

namespace ChronoLog.ChronoLogService.Services;

public class UserServiceFactory
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuthenticationStateProvider _authStateProvider;

    public UserServiceFactory(
        IHttpContextAccessor httpContextAccessor,
        AuthenticationStateProvider authStateProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _authStateProvider = authStateProvider;
    }

    public IUserService Create()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        // API-Request: If HttpContext exists AND no SignalR-Connection
        if (httpContext != null && !httpContext.Request.Path.StartsWithSegments("/_blazor"))
        {
            return new ApiUserService(_httpContextAccessor);
        }

        // Blazor-Component: uses AuthenticationStateProvider
        return new UserService(_authStateProvider);
    }
}