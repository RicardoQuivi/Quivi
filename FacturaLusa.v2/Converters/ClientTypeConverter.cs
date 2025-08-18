using FacturaLusa.v2.Dtos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FacturaLusa.v2.Converters
{
    public class ClientTypeConverter : JsonConverter<CustomerType>
    {
        public override CustomerType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
                return Map(reader.GetString() ?? string.Empty);

            throw new JsonException("Invalid JSON token for decimal");
        }

        public override void Write(Utf8JsonWriter writer, CustomerType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Map(value));
        }

        private string Map(CustomerType value)
        {
            return value switch
            {
                CustomerType.Business => "Empresarial",
                CustomerType.Personal => "Particular",
                _ => throw new NotImplementedException($"Unknown ClientType: {value}"),
            };
        }

        private CustomerType Map(string value)
        {
            return value switch
            {
                "Empresarial" => CustomerType.Business,
                "Particular" => CustomerType.Personal,
                _ => throw new NotImplementedException($"Unknown ClientType: {value}"),
            };
        }
    }
}
