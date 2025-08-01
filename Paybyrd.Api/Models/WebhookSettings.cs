namespace Paybyrd.Api.Models
{
    public class WebhookSettings
    {
        public required string Id { get; set; }
        public required string Url { get; set; }
        public required IEnumerable<string> Events { get; set; }
        public required IEnumerable<PaymentMethod> PaymentMethods { get; set; }
    }
}