using Newtonsoft.Json;
using Quivi.Infrastructure.Pos.Facturalusa.JsonConverters;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Items
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter<ItemFilter>))]
    public enum ItemFilter
    {
        [JsonProperty("ID")]
        Id,

        [JsonProperty("Reference")]
        Reference,

        [JsonProperty("Description")]
        Description,

        [JsonProperty("Barcode")]
        BarCode,
    }
}
