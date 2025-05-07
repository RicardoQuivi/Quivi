using Newtonsoft.Json;
using Quivi.Infrastructure.Pos.Facturalusa.Models.DocumentTypes;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Sales
{
    public class WriteonlySale : ASale
    {
        [JsonProperty("sale_reference_id", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public long? ReferencedSaleId { get; set; }

        [JsonProperty("serie")]
        public long SerieId { get; set; }

        [JsonProperty("document_type")]
        public DocumentType DocumentType { get; set; }

        [JsonProperty("customer")]
        public long CustomerId { get; set; }

        [JsonProperty("vat_number")]
        public string? CustomerVatNumber { get; set; }

        [JsonProperty("address")]
        public string? CustomerAddress { get; set; }

        [JsonProperty("postal_code")]
        public string? CustomerPostalCode { get; set; }

        [JsonProperty("city")]
        public string? CustomerCityName { get; set; }

        [JsonProperty("country")]
        public string? CustomerCountryName { get; set; }

        [JsonProperty("currency")]
        public int CurrencyId { get; set; }

        [JsonProperty("payment_method")]
        public int PaymentMethodId { get; set; }

        [JsonProperty("payment_condition")]
        public int PaymentConditionId { get; set; }

        [JsonProperty("observations")]
        public string? Notes { get; set; }

        [JsonProperty("items")]
        public IEnumerable<WriteonlySaleItem>? Items { get; set; }
    }
}