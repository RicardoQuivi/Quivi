using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Sales
{
    public abstract class ASaleItem
    {
        [JsonProperty("id")]
        public long ItemId { get; set; }

        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }

        [JsonProperty("discount")]
        public decimal DiscountPercentage { get; set; }
    }
}
