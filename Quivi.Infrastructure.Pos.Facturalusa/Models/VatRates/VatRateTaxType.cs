using Newtonsoft.Json;
using Quivi.Infrastructure.Pos.Facturalusa.JsonConverters;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.VatRates
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter<VatRateTaxType>))]
    public enum VatRateTaxType
    {
        [JsonProperty("Normal")]
        Normal,

        [JsonProperty("Intermediária")]
        Intermediary,
        
        [JsonProperty("Isenta")]
        Exempt,
        
        [JsonProperty("Reduzida")]
        Reduced,
    }
}
