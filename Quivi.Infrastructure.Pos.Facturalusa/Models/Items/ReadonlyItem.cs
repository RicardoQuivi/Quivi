using Newtonsoft.Json;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Units;
using Quivi.Infrastructure.Pos.Facturalusa.Models.VatRates;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Items
{
    public class ReadonlyItem : AItem
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("vat")]
        public VatRate? VatRate { get; set; }

        [JsonProperty("unit")]
        public Unit? Unit { get; set; }

        [JsonProperty("active")]
        public bool IsActive { get; set; }

        [JsonIgnore]
        public bool IsGenericItem => Name == DefaultCode;
    }
}
