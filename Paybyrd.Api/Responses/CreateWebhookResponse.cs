using Paybyrd.Api.Models;

namespace Paybyrd.Api.Responses
{
    public class CreateWebhookResponse
    {
        public required WebhookSettings Data { get; init; }
    }
}