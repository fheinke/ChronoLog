using ChronoLog.Core.Models.DisplayObjects;

namespace ChronoLog.Core.Interfaces;

public interface ITimeEntryService
{
    Task<Guid> CreateTimeEntryAsync(TimeEntryModel timeEntry);
    Task<List<TimeEntryModel>> GetTimeEntriesAsync();
    Task<List<TimeEntryModel>> GetTimeEntriesAsync(List<Guid> timeEntryIds);
    Task<List<TimeEntryModel>> GetTimeEntriesAsync(DateTime startDate, DateTime endDate);
    Task<TimeEntryModel?> GetTimeEntryAsync(Guid timeEntryId);
    Task<bool> UpdateTimeEntryAsync(TimeEntryModel timeEntry);
    Task<bool> DeleteTimeEntryAsync(Guid timeEntryId);
}