using System.Text.Json.Serialization;

namespace Paybyrd.Api.Models
{
    public class Payment
    {
        public string TransactionId { get; set; } = string.Empty;
        public string OrderRef { get; set; } = string.Empty;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentStatus Status { get; set; }
    }
}