using FacturaLusa.v2.Dtos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FacturaLusa.v2.Converters
{
    public class SaleStatusConverter : JsonConverter<SaleStatus>
    {
        public override SaleStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
                return Map(reader.GetString() ?? string.Empty);

            throw new JsonException("Invalid JSON token for decimal");
        }

        public override void Write(Utf8JsonWriter writer, SaleStatus value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Map(value));
        }

        private string Map(SaleStatus value)
        {
            return value switch
            {
                SaleStatus.Draft => "Rascunho",
                SaleStatus.Final => "Terminado",
                SaleStatus.Voided => "Anulado",
                _ => throw new NotImplementedException($"Unknown SaleStatus: {value}"),
            };
        }

        private SaleStatus Map(string value)
        {
            return value switch
            {
                "Rascunho" => SaleStatus.Draft,
                "Terminado" => SaleStatus.Final,
                "Anulado" => SaleStatus.Voided,
                _ => throw new NotImplementedException($"Unknown SaleStatus: {value}"),
            };
        }
    }
}
