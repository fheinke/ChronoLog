using System.Security.Claims;
using ChronoLog.Core.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;

namespace ChronoLog.Applications.Services;

public class UserService : IUserService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public UserService(AuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationStateProvider = authenticationStateProvider;
    }

    private async Task<ClaimsPrincipal> GetUserAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        return authState.User;
    }

    public async Task<string?> GetUserNameAsync()
    {
        var user = await GetUserAsync();
        return user.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
    }

    public async Task<string?> GetUserEmailAsync()
    {
        var user = await GetUserAsync();
        return user.FindFirst("preferred_username")?.Value
               ?? user.FindFirst(ClaimTypes.Email)?.Value;
    }

    public async Task<string?> GetUserIdAsync()
    {
        var user = await GetUserAsync();
        return user.FindFirst("oid")?.Value // Entra AD Object ID
               ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public async Task<List<string>> GetUserRolesAsync()
    {
        var user = await GetUserAsync();
        return user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
    }
}