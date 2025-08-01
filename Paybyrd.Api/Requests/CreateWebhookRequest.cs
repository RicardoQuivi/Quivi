using Paybyrd.Api.Models;

namespace Paybyrd.Api.Requests
{
    public class CreateWebhookRequest
    {
        public required string Url { get; init; }
        public required CredentialType CredentialType { get; init; }
        public IEnumerable<string>? Events { get; init; }
        public IEnumerable<PaymentMethod>? PaymentMethods { get; init; }
    }
}
