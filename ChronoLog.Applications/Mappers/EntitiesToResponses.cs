using ChronoLog.Core.Models.DTOs;
using ChronoLog.SqlDatabase.Models;

namespace ChronoLog.Applications.Mappers;

public static class EntitiesToResponses
{
    public static WorkdayResponse ToResponse(this WorkdayEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new WorkdayResponse(
            entity.WorkdayId,
            entity.EmployeeId,
            entity.Date,
            entity.Type,
            entity.Worktimes.Select(wt => wt.ToModel()).ToList(),
            entity.TimeEntries.Select(pt => pt.ToModel()).ToList()
        );
    }
}