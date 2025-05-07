using Newtonsoft.Json;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Items;
using Quivi.Infrastructure.Pos.Facturalusa.Models.VatRates;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Sales
{
    public class ReadonlySaleItem : ASaleItem
    {
        [JsonProperty("unit_price")]
        public decimal Price { get; set; }

        [JsonProperty("vat")]
        public required VatRate VatRate { get; set; }

        [JsonProperty("item_details")]
        public string? Description { get; set; }
        
        [JsonProperty("item")]
        public ReadonlySaleItemDetails? Details { get; set; }
    }

    public class ReadonlySaleItemDetails
    {
        [JsonProperty("reference")]
        public string? Reference { get; set; }

        [JsonProperty("description")]
        public string? Name { get; set; }

        [JsonProperty("type")]
        public ItemType Type { get; set; }
    }
}