using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Units
{
    public class GetUnitsRequest : ARequestBase
    {
        [JsonProperty("value")]
        public string? Name { get; set; }
    }
}