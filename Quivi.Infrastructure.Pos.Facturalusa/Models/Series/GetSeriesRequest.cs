using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Series
{
    public class GetSeriesRequest : ARequestBase
    {
        [JsonProperty("value")]
        public string? Name { get; set; }
    }
}
