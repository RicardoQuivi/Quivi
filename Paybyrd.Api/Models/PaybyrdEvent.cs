using Paybyrd.Api.Converters;
using System.Text.Json.Serialization;

namespace Paybyrd.Api.Models
{
    [JsonConverter(typeof(PaybyrdEventConverter))]
    public class PaybyrdEvent
    {
        public string Id { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string Event { get; set; } = string.Empty;
        public string SettingsId { get; set; } = string.Empty;
        public string AttemptId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public object? Content { get; set; }
    }
}