using ChronoLog.Core.Models;

namespace ChronoLog.Core;

public static class Extensions
{
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