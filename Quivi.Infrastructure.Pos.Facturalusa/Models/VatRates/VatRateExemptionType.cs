using Newtonsoft.Json;
using Quivi.Infrastructure.Pos.Facturalusa.JsonConverters;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.VatRates
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter<VatRateExemptionType>))]
    public enum VatRateExemptionType
    {
        [JsonProperty("M18")]
        NoExemption = 0,

        [JsonProperty("M19")]
        GenericExemption,
    }
}
