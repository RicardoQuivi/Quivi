using FacturaLusa.v2.Dtos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FacturaLusa.v2.Converters
{
    public class VatTypeConverter : JsonConverter<VatType>
    {
        public override VatType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
                return Map(reader.GetString() ?? string.Empty);

            throw new JsonException("Invalid JSON token for decimal");
        }

        public override void Write(Utf8JsonWriter writer, VatType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Map(value));
        }

        private string Map(VatType value)
        {
            return value switch
            {
                VatType.VatIncluded => "IVA incluído",
                VatType.VatDebit => "Debitar IVA",
                VatType.NoAction => "Não fazer nada",
                _ => throw new NotImplementedException($"Unknown VatType: {value}"),
            };
        }

        private VatType Map(string value)
        {
            return value switch
            {
                "IVA incluído" => VatType.VatIncluded,
                "Debitar IVA" => VatType.VatDebit,
                "Não fazer nada" => VatType.NoAction,
                _ => throw new NotImplementedException($"Unknown VatType: {value}"),
            };
        }
    }
}
