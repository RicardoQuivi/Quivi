using FacturaLusa.v2.Dtos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FacturaLusa.v2.Converters
{
    public class SaftRegionConverter : JsonConverter<SaftRegion>
    {
        public override SaftRegion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
                return Map(reader.GetString() ?? string.Empty);

            throw new JsonException("Invalid JSON token for decimal");
        }

        public override void Write(Utf8JsonWriter writer, SaftRegion value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Map(value));
        }

        private string Map(SaftRegion value)
        {
            return value switch
            {
                SaftRegion.PortugalContinental => "PT",
                SaftRegion.Açores => "PT-AC",
                SaftRegion.Madeira => "PT-MA",
                _ => throw new NotImplementedException($"Unknown SaftRegion: {value}"),
            };
        }

        private SaftRegion Map(string value)
        {
            return value switch
            {
                "PT" => SaftRegion.PortugalContinental,
                "PT-AC" => SaftRegion.Açores,
                "PT-MA" => SaftRegion.Madeira,
                _ => throw new NotImplementedException($"Unknown SaftRegion: {value}"),
            };
        }
    }
}
