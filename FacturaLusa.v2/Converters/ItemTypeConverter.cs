using FacturaLusa.v2.Dtos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FacturaLusa.v2.Converters
{
    public class ItemTypeConverter : JsonConverter<ItemType>
    {
        public override ItemType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
                return Map(reader.GetString() ?? string.Empty);

            throw new JsonException("Invalid JSON token for decimal");
        }

        public override void Write(Utf8JsonWriter writer, ItemType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Map(value));
        }

        private string Map(ItemType value)
        {
            return value switch
            {
                ItemType.Service => "Serviços",
                ItemType.Product => "Produtos acabados e intermédios",
                _ => throw new NotImplementedException($"Unknown ItemType: {value}"),
            };
        }

        private ItemType Map(string value)
        {
            return value switch
            {
                "Serviços" => ItemType.Service,
                "Produtos acabados e intermédios" => ItemType.Product,
                _ => throw new NotImplementedException($"Unknown ItemType: {value}"),
            };
        }
    }
}
