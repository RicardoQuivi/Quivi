using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Units
{
    public class Unit
    {
        [JsonProperty("ID")]
        public int Id { get; set; }

        [JsonProperty("description")]
        public required string Name { get; set; }

        [JsonProperty("symbol")]
        public required string Symbol { get; set; }

        [JsonProperty("active")]
        public bool IsActive { get; set; }
    }
}
