using System.Security.Claims;
using ChronoLog.Applications.Mappers;
using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.SqlDatabase.Context;
using Microsoft.EntityFrameworkCore;

namespace ChronoLog.ChronoLogService.Authorization;

public interface IApiUserService
{
    Task<EmployeeModel?> GetCurrentEmployeeAsync(ClaimsPrincipal user);
}

public class ApiUserService : IApiUserService
{
    private readonly SqlDbContext _sqlDbContext;

    public ApiUserService(SqlDbContext sqlDbContext)
    {
        _sqlDbContext = sqlDbContext;
    }
    
    public async Task<EmployeeModel?> GetCurrentEmployeeAsync(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst("oid")?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim))
            return null;

        var employee = await _sqlDbContext.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ObjectId == userIdClaim);
        
        return employee?.ToModel() ?? null;
    }
}