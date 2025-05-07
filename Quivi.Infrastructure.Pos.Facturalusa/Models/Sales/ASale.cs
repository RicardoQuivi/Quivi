using Newtonsoft.Json;
using Quivi.Infrastructure.Pos.Facturalusa.JsonConverters;
using Quivi.Infrastructure.Pos.Facturalusa.Models.VatRates;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Sales
{
    public abstract class ASale
    {
        [JsonProperty("issue_date")]
        [JsonConverter(typeof(JsonDateOnlyConverter))]
        public DateTime IssueDay { get; set; }

        [JsonProperty("vat_type")]
        public VatRateType VatType { get; set; }

        [JsonProperty("status")]
        public SaleStatus Status { get; set; }

        [JsonProperty("canceled_at")]
        [JsonConverter(typeof(JsonDateOnlyConverter))]
        public DateTime? CanceledAt { get; set; }
    }
}
