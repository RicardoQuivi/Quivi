using Newtonsoft.Json;
using Quivi.Infrastructure.Pos.Facturalusa.JsonConverters;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.DocumentTypes
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter<DocumentType>))]
    public enum DocumentType
    {
        [JsonProperty("Factura")]
        Invoice,

        [JsonProperty("Factura Recibo")]
        InvoiceReceipt,

        [JsonProperty("Factura Simplificada")]
        SimplifiedInvoice,

        [JsonProperty("Nota de Crédito")]
        CreditNote,

        [JsonProperty("Nota de Débito")]
        DebitNote,

        [JsonProperty("Factura Pró-forma")]
        ProFormInvoice,

        [JsonProperty("Orçamento")]
        Budget,

        [JsonProperty("Encomenda")]
        Order,

        [JsonProperty("Guia de Transporte")]
        TransportGuide,

        [JsonProperty("Guia de Remessa")]
        ShippingGuide,

        [JsonProperty("Guia de Consignação")]
        ConsignmentGuide,

        [JsonProperty("Guia de Devolução")]
        ReturnGuide,

        [JsonProperty("Guia de movimentação de activos próprios")]
        OwnAssetMovementGuide,

        [JsonProperty("Consulta de Mesa")]
        ConsumerBill,
    }
}
