using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Series
{
    public class CheckCommunicateSerieResponse : AResponseBase
    {
        [JsonProperty("status")]
        public bool AlreadyCommunicated { get; set; }
    }
}