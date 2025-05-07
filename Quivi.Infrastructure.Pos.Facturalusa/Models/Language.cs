using Newtonsoft.Json;
using Quivi.Infrastructure.Pos.Facturalusa.JsonConverters;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter<Language>))]
    public enum Language
    {
        [JsonProperty("PT")]
        Portuguese,

        [JsonProperty("EN")]
        English,
    }
}
