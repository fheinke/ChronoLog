using Microsoft.AspNetCore.Authorization;

namespace ChronoLog.ChronoLogService.Authorization;

public class ProjectManagementRequirement : IAuthorizationRequirement;

public class ProjectManagementHandler : AuthorizationHandler<ProjectManagementRequirement>
{
    private readonly IApiUserService _apiUserService;

    public ProjectManagementHandler(IApiUserService apiUserService)
    {
        _apiUserService = apiUserService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        ProjectManagementRequirement requirement)
    {
        var employee = await _apiUserService.GetCurrentEmployeeAsync(context.User);

        if (employee?.IsAdmin == true || employee?.IsProjectManager == true)
        {
            context.Succeed(requirement);
        }
    }
}