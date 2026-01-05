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
    Feiertag,
    Dienstreise
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReducingWorkdayType
{
    Krankheitstag,
    Urlaub,
    Gleitzeittag,
    Feiertag
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GermanProvince
{
    ALL, // National
    BW, // Baden-Württemberg
    BY, // Bayern
    BE, // Berlin
    BB, // Brandenburg
    HB, // Bremen
    HH, // Hamburg
    HE, // Hessen
    MV, // Mecklenburg-Vorpommern
    NI, // Niedersachsen
    NW, // Nordrhein-Westfalen
    RP, // Rheinland-Pfalz
    SL, // Saarland
    SN, // Sachsen
    ST, // Sachsen-Anhalt
    SH, // Schleswig-Holstein
    TH  // Thüringen
}