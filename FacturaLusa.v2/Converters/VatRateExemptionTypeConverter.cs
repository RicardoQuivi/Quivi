using FacturaLusa.v2.Dtos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FacturaLusa.v2.Converters
{
    public class VatRateExemptionTypeConverter : JsonConverter<VatExemptionType>
    {
        public override VatExemptionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
                return Map(reader.GetString() ?? string.Empty);

            throw new JsonException("Invalid JSON token for decimal");
        }

        public override void Write(Utf8JsonWriter writer, VatExemptionType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Map(value));
        }

        private string Map(VatExemptionType value)
        {
            return value switch
            {
                VatExemptionType.NoExemption => "M18",
                VatExemptionType.GenericExemption => "M19",
                _ => throw new NotImplementedException($"Unknown VatRateExemptionType: {value}"),
            };
        }

        private VatExemptionType Map(string value)
        {
            return value switch
            {
                "M18" => VatExemptionType.NoExemption,
                "M19" => VatExemptionType.GenericExemption,
                _ => throw new NotImplementedException($"Unknown VatRateExemptionType: {value}"),
            };
        }
    }
}
