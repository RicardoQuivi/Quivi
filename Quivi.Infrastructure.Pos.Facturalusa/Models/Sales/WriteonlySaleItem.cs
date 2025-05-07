using Newtonsoft.Json;
using Quivi.Infrastructure.Pos.Facturalusa.Models.VatRates;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Sales
{
    public class WriteonlySaleItem : ASaleItem
    {
        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("vat")]
        public int VatRateId { get; set; }

        [JsonProperty("details")]
        public string? Description { get; set; }

        [JsonProperty("vat_exemption")]
        public VatRateExemptionType? VatRateExemption { get; set; }
    }
}