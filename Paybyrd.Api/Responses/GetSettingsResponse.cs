using Paybyrd.Api.Models;

namespace Paybyrd.Api.Responses
{
    public class GetSettingsResponse
    {
        public required IEnumerable<WebhookSettings> Data { get; init; }
    }
}