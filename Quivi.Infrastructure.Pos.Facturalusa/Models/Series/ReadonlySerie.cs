using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Series
{
    public class ReadonlySerie : ASerie
    {
        [JsonProperty("id")]
        public long Id { get; set; }
    }
}