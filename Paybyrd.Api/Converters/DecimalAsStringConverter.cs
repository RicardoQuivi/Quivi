using System.Text.Json;
using System.Text.Json.Serialization;

namespace Paybyrd.Api.Converters
{
    public class DecimalAsStringConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Accept both string and number input
            if (reader.TokenType == JsonTokenType.String && decimal.TryParse(reader.GetString(), out var result))
            {
                return result;
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDecimal();
            }

            throw new JsonException("Invalid JSON token for decimal");
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("0.##")); // Or "G" for general format
        }
    }
}