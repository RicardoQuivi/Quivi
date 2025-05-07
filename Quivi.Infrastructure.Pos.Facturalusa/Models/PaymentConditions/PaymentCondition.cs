using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.PaymentConditions
{
    public class PaymentCondition
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("description")]
        public string? Name { get; set; }
        
        [JsonProperty("days")]
        public int Days { get; set; }
        
        [JsonProperty("active")]
        public bool IsActive { get; set; }
    }
}
