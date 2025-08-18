using FacturaLusa.v2.Dtos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FacturaLusa.v2.Converters
{
    public class SearchFieldConverter : JsonConverter<SearchField>
    {
        public override SearchField Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
                return Map(reader.GetString() ?? string.Empty);

            throw new JsonException("Invalid JSON token for decimal");
        }

        public override void Write(Utf8JsonWriter writer, SearchField value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Map(value));
        }

        private string Map(SearchField value)
        {
            return value switch
            {
                SearchField.Id => "ID",
                SearchField.Barcode => "Barcode",
                SearchField.Description => "Description",
                SearchField.Reference => "Reference",
                SearchField.Code => "Code",
                SearchField.VatNumber => "Vat Number",
                SearchField.DocumentNumber => "Document Number",
                _ => throw new NotImplementedException($"Unknown SearchField: {value}"),
            };
        }

        private SearchField Map(string value)
        {
            return value switch
            {
                "ID" => SearchField.Id,
                "Barcode" => SearchField.Barcode,
                "Description" => SearchField.Description,
                "Reference" => SearchField.Reference,
                "Code" => SearchField.Code,
                "Vat Number" => SearchField.VatNumber,
                "Document Number" => SearchField.DocumentNumber,
                _ => throw new NotImplementedException($"Unknown SearchField: {value}"),
            };
        }
    }
}