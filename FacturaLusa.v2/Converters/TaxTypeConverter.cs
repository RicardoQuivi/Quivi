using FacturaLusa.v2.Dtos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FacturaLusa.v2.Converters
{
    public class TaxTypeConverter : JsonConverter<TaxType>
    {
        public override TaxType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
                return Map(reader.GetString() ?? string.Empty);

            throw new JsonException("Invalid JSON token for decimal");
        }

        public override void Write(Utf8JsonWriter writer, TaxType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Map(value));
        }

        private string Map(TaxType value)
        {
            return value switch
            {
                TaxType.Normal => "Normal",
                TaxType.Intermedia => "Intermédia",
                TaxType.Isenta => "Isenta",
                TaxType.Reduzida => "Reduzida",
                _ => throw new NotImplementedException($"Unknown TaxType: {value}"),
            };
        }

        private TaxType Map(string value)
        {
            return value switch
            {
                "Normal" => TaxType.Normal,
                "Intermédia" => TaxType.Intermedia,
                "Isenta" => TaxType.Isenta,
                "Reduzida" => TaxType.Reduzida,
                _ => throw new NotImplementedException($"Unknown TaxType: {value}"),
            };
        }
    }
}
