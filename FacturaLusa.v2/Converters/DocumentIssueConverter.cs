using FacturaLusa.v2.Dtos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FacturaLusa.v2.Converters
{
    public class DocumentIssueConverter : JsonConverter<DocumentIssue>
    {
        public override DocumentIssue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
                return Map(reader.GetString() ?? string.Empty);

            throw new JsonException("Invalid JSON token for decimal");
        }

        public override void Write(Utf8JsonWriter writer, DocumentIssue value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Map(value));
        }

        private string Map(DocumentIssue value)
        {
            return value switch
            {
                DocumentIssue.SecondCopy => "2ª via",
                DocumentIssue.Original => "Original",
                _ => throw new NotImplementedException($"Unknown DocumentIssue: {value}"),
            };
        }

        private DocumentIssue Map(string value)
        {
            return value switch
            {
                "2ª via" => DocumentIssue.SecondCopy,
                "Original" => DocumentIssue.Original,
                _ => throw new NotImplementedException($"Unknown DocumentIssue: {value}"),
            };
        }
    }
}
