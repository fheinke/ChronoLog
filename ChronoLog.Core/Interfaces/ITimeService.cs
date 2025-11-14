namespace ChronoLog.Core.Interfaces;

public interface ITimeService
{
    Task<bool> CreateEntryAsync(DateTimeOffset dateTimeOffset);
}