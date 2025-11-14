using System.Text.Json.Serialization;

namespace ChronoLog.Core.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkdayType
{
    Homeoffice,
    Office,
    Krankheitstag,
    Urlaub,
    Gleitzeittag,
    Dienstreise
}