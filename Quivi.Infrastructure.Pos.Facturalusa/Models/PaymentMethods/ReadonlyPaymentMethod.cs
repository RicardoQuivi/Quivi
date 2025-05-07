using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.PaymentMethods
{
    public class ReadonlyPaymentMethod : APaymentMethod
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("saft_initials")]
        public string? SaftInitials { get; set; }
    }
}