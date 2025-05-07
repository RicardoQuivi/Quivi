using Newtonsoft.Json;
using Quivi.Infrastructure.Pos.Facturalusa.JsonConverters;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Sales
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter<SaleFilter>))]
    public enum SaleFilter
    {
        [JsonProperty("ID")]
        Id,

        /// <summary>
        /// Format example: FR EX/173
        /// </summary>
        [JsonProperty("Document Number")]
        DocumentNumber,
    }
}