using ChronoLog.Core.Interfaces;
using ChronoLog.SqlDatabase.Context;

namespace ChronoLog.Applications.Services;

public class TimeService : ITimeService
{
    private readonly SqlDbContext _sqlDbContext;

    public TimeService(SqlDbContext sqlDbContext)
    {
        _sqlDbContext = sqlDbContext;
    }

    public async Task<bool> CreateEntryAsync(DateTimeOffset dateTimeOffset)
    {
        throw new NotImplementedException();
    }
}