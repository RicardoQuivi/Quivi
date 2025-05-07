using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Series
{
    public abstract class ASerie
    {
        [JsonProperty("description")]
        public string? Name { get; set; }

        [JsonProperty("valid_until")]
        public int ExpirationYear { get; set; }
        
        [JsonProperty("active")]
        public bool IsActive { get; set; }
    }
}
