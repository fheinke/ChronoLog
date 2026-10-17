using ChronoLog.Core.Models;

namespace ChronoLog.Core;

public static class Extensions
{
    private const string DefaultWorkingDays = "1,2,3,4,5";

    public static List<DayOfWeek> ParseWorkingDays(string? value)
    {
        var days = (string.IsNullOrWhiteSpace(value) ? DefaultWorkingDays : value)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var d) && d is >= 0 and <= 6 ? (DayOfWeek?)d : null)
            .Where(d => d.HasValue)
            .Select(d => d!.Value)
            .Distinct()
            .OrderBy(d => d == DayOfWeek.Sunday ? 7 : (int)d)
            .ToList();
        return days;
    }

    public static string SerializeWorkingDays(IEnumerable<DayOfWeek>? days) =>
        days is null ? DefaultWorkingDays : string.Join(',', days.Distinct().Select(d => (int)d));

    public static List<T> GetAll<T>() where T : struct, Enum
    {
        return Enum.GetValues<T>().ToList();
    }
    
    extension(WorkdayType type)
    {
        public bool IsWorkingDay() => type switch
        {
            WorkdayType.Homeoffice => true,
            WorkdayType.Office => true,
            WorkdayType.Dienstreise => true,
            _ => false
        };

        public bool IsNonWorkingDay() => type switch
        {
            WorkdayType.Krankheitstag => true,
            WorkdayType.Urlaub => true,
            WorkdayType.Gleitzeittag => true,
            WorkdayType.Feiertag => true,
            _ => false
        };
    }
}