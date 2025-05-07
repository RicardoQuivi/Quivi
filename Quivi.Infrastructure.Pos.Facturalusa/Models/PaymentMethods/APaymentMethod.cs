using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.PaymentMethods
{
    public abstract class APaymentMethod
    {
        [JsonProperty("description")]
        public string? Name { get; set; }

        [JsonProperty("description_english")]
        public string? EnglishName { get; set; }

        [JsonProperty("active")]
        public bool IsActive { get; set; }
    }
}
