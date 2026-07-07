using System.Text.Json;
using System.Text.Json.Serialization;

namespace GCTL_App.Converters
{
    public class DateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.GetString() is string s && DateTime.TryParse(s, out var dt) ? dt : null;

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString("dd/MM/yyyy"));
            else
                writer.WriteNullValue();
        }
    }
}