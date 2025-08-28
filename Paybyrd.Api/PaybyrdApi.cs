using Paybyrd.Api.Converters;
using Paybyrd.Api.Requests;
using Paybyrd.Api.Responses;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Paybyrd.Api
{
    public class PaybyrdApi : IPaybyrdApi, IPaybyrdWebhooksApi
    {
        private readonly Uri apiAdress;
        private readonly Uri webHooksAddress;
        private readonly JsonSerializerOptions jsonSerializerOptions;

        public PaybyrdApi(string apiAddress, string webHooksAddress)
        {
            this.apiAdress = new Uri(apiAddress);
            this.webHooksAddress = new Uri(webHooksAddress);

            jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
            };
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            jsonSerializerOptions.Converters.Add(new DecimalAsStringConverter());
            jsonSerializerOptions.Converters.Add(new IntAsStringConverter());
        }

        private async Task Delete(Uri baseAddress, string endpoint, string apiKey)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = baseAddress;
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

            var response = await httpClient.DeleteAsync(endpoint);
            return;
        }


        private async Task<TResponse> Get<TResponse>(Uri baseAddress, string endpoint, string apiKey)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = baseAddress;
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

            var response = await httpClient.GetAsync(endpoint);
            var rawResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TResponse>(rawResponse, jsonSerializerOptions) ?? throw new InvalidOperationException("Failed to deserialize response from Paybyrd API.");
            return result;
        }


        private async Task<TResponse> Post<TResponse>(Uri baseAddress, string endpoint, string apiKey, object request)
        {
            var json = JsonSerializer.Serialize(request, jsonSerializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpClient = new HttpClient();
            httpClient.BaseAddress = baseAddress;
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

            var response = await httpClient.PostAsync(endpoint, content);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception("Paybyrd is not available");

            var rawResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TResponse>(rawResponse, jsonSerializerOptions) ?? throw new InvalidOperationException("Failed to deserialize response from Paybyrd API.");
            return result;
        }

        #region IPabyrdApi
        public Task<CreatePaymentResponse> CreatePayment(string apiKey, CreatePaymentRequest request) => Post<CreatePaymentResponse>(apiAdress, "/api/v2/payment", apiKey, request);
        public Task<CapturePaymentTransactionResponse> CapturePayment(string apiKey, CapturePaymentTransactionRequest request) => Post<CapturePaymentTransactionResponse>(apiAdress, $"/api/v2/capture/{request.TransactionId}", apiKey, request);
        public Task<GetPaymentResponse> GetPayment(string apiKey, GetPaymentRequest request) => Get<GetPaymentResponse>(apiAdress, $"api/v2/transactions/{request.TransactionId}", apiKey);
        public Task<RefundPaymentResponse> RefundPayment(string apiKey, RefundPaymentRequest request) => Post<RefundPaymentResponse>(apiAdress, $"/api/v2/refund/{request.TransactionId}", apiKey, request);

        public Task<CreateOrderResponse> CreateOrder(string apiKey, CreateOrderRequest request) => Post<CreateOrderResponse>(apiAdress, $"/api/v2/orders", apiKey, request);
        public Task<GetOrderResponse> GetOrder(string apiKey, GetOrderRequest request) => Get<GetOrderResponse>(apiAdress, $"api/v2/orders/{request.OrderId}", apiKey);
        #endregion

        #region IPaybyrdWebhooksApi
        public Task<GetSettingsResponse> GetSettings(string apiKey) => Get<GetSettingsResponse>(webHooksAddress, $"/api/v1/settings", apiKey);
        public Task<CreateWebhookResponse> Create(string apiKey, CreateWebhookRequest request) => Post<CreateWebhookResponse>(webHooksAddress, $"/api/v1/settings", apiKey, request);
        public Task Delete(string apiKey, DeleteWebhookRequest request) => Delete(webHooksAddress, $"api/v1/settings/{request.WebhookId}", apiKey);
        #endregion
    }
}