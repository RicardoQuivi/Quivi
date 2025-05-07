using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.VatRates
{
    public class VatRate
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("description")]
        public required string Name { get; set; }

        [JsonProperty("tax")]
        public decimal PercentageTax { get; set; }

        [JsonProperty("type")]
        public VatRateTaxType TaxType { get; set; }

        [JsonProperty("saft_region")]
        public string SaftRegion => "PT";

        [JsonProperty("active")]
        public bool IsActive { get; set; }
    }
}
