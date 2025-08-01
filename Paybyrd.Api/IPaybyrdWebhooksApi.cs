using Paybyrd.Api.Requests;
using Paybyrd.Api.Responses;

namespace Paybyrd.Api
{
    public interface IPaybyrdWebhooksApi
    {
        Task<GetSettingsResponse> GetSettings(string apiKey);
        Task<CreateWebhookResponse> Create(string apiKey, CreateWebhookRequest request);
        Task Delete(string apiKey, DeleteWebhookRequest request);
    }
}