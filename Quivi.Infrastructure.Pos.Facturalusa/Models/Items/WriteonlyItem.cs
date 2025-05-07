using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Items
{
    public class WriteonlyItem : AItem
    {
        [JsonProperty("vat")]
        public int VatId { get; set; }

        [JsonProperty("unit")]
        public int UnitId { get; set; }
    }
}
