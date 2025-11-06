using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Snapdi.Api.Converters
{
    /// <summary>
    /// Custom JSON converter that ensures all DateTime values are treated as UTC
    /// </summary>
    public class UtcDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateTimeString = reader.GetString();
            if (string.IsNullOrEmpty(dateTimeString))
            {
                return default;
            }

            // Parse the DateTime
            if (DateTime.TryParse(dateTimeString, out DateTime dateTime))
            {
                // If the parsed DateTime has Kind=Unspecified or Local, convert to UTC
                if (dateTime.Kind == DateTimeKind.Unspecified)
                {
                    // Assume it's UTC if no timezone info
                    return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                }
                else if (dateTime.Kind == DateTimeKind.Local)
                {
                    return dateTime.ToUniversalTime();
                }
                
                return dateTime; // Already UTC
            }

            throw new JsonException($"Unable to parse '{dateTimeString}' as DateTime");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Always write as UTC in ISO 8601 format with 'Z' suffix
            var utcValue = value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : value;
            writer.WriteStringValue(utcValue.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        }
    }

    /// <summary>
    /// Custom JSON converter for nullable DateTime that ensures all values are treated as UTC
    /// </summary>
    public class UtcNullableDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateTimeString = reader.GetString();
            if (string.IsNullOrEmpty(dateTimeString))
            {
                return null;
            }

            // Parse the DateTime
            if (DateTime.TryParse(dateTimeString, out DateTime dateTime))
            {
                // If the parsed DateTime has Kind=Unspecified or Local, convert to UTC
                if (dateTime.Kind == DateTimeKind.Unspecified)
                {
                    // Assume it's UTC if no timezone info
                    return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                }
                else if (dateTime.Kind == DateTimeKind.Local)
                {
                    return dateTime.ToUniversalTime();
                }
                
                return dateTime; // Already UTC
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                // Always write as UTC in ISO 8601 format with 'Z' suffix
                var utcValue = value.Value.Kind == DateTimeKind.Local ? value.Value.ToUniversalTime() : value.Value;
                writer.WriteStringValue(utcValue.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
