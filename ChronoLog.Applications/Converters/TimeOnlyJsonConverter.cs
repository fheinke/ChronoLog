using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChronoLog.Applications.Converters
{
    public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
    {
        private const string WriteFormat = "HH:mm:ss.fffffff";
        private static readonly string[] AcceptedFormats = new[] { WriteFormat, "HH:mm:ss.FFFFFFF", "HH:mm:ss" };

        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                throw new JsonException("TimeOnly value cannot be null.");

            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
                throw new JsonException("TimeOnly value cannot be null or empty.");

            var s = value.Trim();

            // 1) Exakte TimeOnly-Formate
            if (TimeOnly.TryParseExact(s, AcceptedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var timeOnly))
                return timeOnly;

            // 2) Allgemeines TimeOnly-Parsing (z.B. "20:23:39")
            if (TimeOnly.TryParse(s, CultureInfo.InvariantCulture, out timeOnly))
                return timeOnly;

            // 3) Fallback: ISO-Zeitstempel oder Datum+Uhrzeit mit Offset
            //    Akzeptiert z.B. "20:23:39.888Z" oder "2025-12-10T20:23:39.888+01:00"
            if (DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dto))
                return TimeOnly.FromTimeSpan(dto.TimeOfDay);

            throw new JsonException($"Invalid TimeOnly format. Expected formats: {string.Join(", ", AcceptedFormats)}; got: {value}");
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(WriteFormat, CultureInfo.InvariantCulture));
        }
    }
}
