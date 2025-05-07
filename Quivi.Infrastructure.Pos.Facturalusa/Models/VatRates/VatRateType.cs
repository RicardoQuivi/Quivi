using Newtonsoft.Json;
using Quivi.Infrastructure.Pos.Facturalusa.JsonConverters;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.VatRates
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter<VatRateType>))]
    public enum VatRateType
    {
        [JsonProperty("Não fazer nada")]
        DoNothing = 0,

        [JsonProperty("Debitar IVA")]
        DebitVAT,

        [JsonProperty("IVA")]
        VAT,

        [JsonProperty("IVA incluído")]
        IncludedVAT,
    }
}
