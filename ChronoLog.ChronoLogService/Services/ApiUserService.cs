using System.Security.Claims;
using ChronoLog.Core.Interfaces;

namespace ChronoLog.ChronoLogService.Services;

public class ApiUserService : IUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal GetUser()
    {
        return _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();
    }

    public Task<string?> GetUserNameAsync()
    {
        var user = GetUser();
        return Task.FromResult(user.Claims.FirstOrDefault(c => c.Type == "name")?.Value);
    }

    public Task<string?> GetUserEmailAsync()
    {
        var user = GetUser();
        return Task.FromResult(user.FindFirst("preferred_username")?.Value
                               ?? user.FindFirst(ClaimTypes.Email)?.Value);
    }

    public Task<string?> GetUserIdAsync()
    {
        var user = GetUser();
        return Task.FromResult(user.FindFirst("oid")?.Value
                               ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    }
}