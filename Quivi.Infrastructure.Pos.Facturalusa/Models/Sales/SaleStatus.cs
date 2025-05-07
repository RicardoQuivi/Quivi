using Newtonsoft.Json;
using Quivi.Infrastructure.Pos.Facturalusa.JsonConverters;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Sales
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter<SaleStatus>))]
    public enum SaleStatus
    {
        [JsonProperty("Rascunho")]
        Draft,

        [JsonProperty("Terminado")]
        Final,
    }
}