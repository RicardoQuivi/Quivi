using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Currencies
{
    public class Currency
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("description")]
        public string? Name { get; set; }

        [JsonProperty("symbol")]
        public string? Symbol { get; set; }

        [JsonProperty("code_iso")]
        public string? IsoCode { get; set; }

        [JsonProperty("exchange_sale")]
        public decimal Exchange { get; set; }

        [JsonProperty("is_default")]
        public bool IsDefault { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }
    }
}