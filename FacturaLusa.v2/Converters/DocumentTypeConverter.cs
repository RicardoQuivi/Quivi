using FacturaLusa.v2.Dtos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FacturaLusa.v2.Converters
{
    public class DocumentTypeConverter : JsonConverter<DocumentType>
    {
        public override DocumentType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
                return Map(reader.GetString() ?? string.Empty);

            throw new JsonException("Invalid JSON token for decimal");
        }

        public override void Write(Utf8JsonWriter writer, DocumentType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Map(value));
        }

        private string Map(DocumentType value)
        {
            return value switch
            {
                DocumentType.Invoice => "Factura",
                DocumentType.InvoiceReceipt => "Factura Recibo",
                DocumentType.SimplifiedInvoice => "Factura Simplificada",
                DocumentType.CreditNote => "Nota de Crédito",
                DocumentType.ConsumerBill => "Consulta de Mesa",
                _ => throw new NotImplementedException($"Unknown DocumentType: {value}"),
            };
        }

        private DocumentType Map(string value)
        {
            return value switch
            {
                "Factura" => DocumentType.Invoice,
                "Factura Recibo" => DocumentType.InvoiceReceipt,
                "Factura Simplificada" => DocumentType.SimplifiedInvoice,
                "Nota de Crédito" => DocumentType.CreditNote,
                "Consulta de Mesa" => DocumentType.ConsumerBill,
                _ => throw new NotImplementedException($"Unknown DocumentType: {value}"),
            };
        }
    }
}