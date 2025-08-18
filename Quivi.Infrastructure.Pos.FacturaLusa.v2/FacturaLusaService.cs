using FacturaLusa.v2;
using FacturaLusa.v2.Dtos.Requests.Currencies;
using FacturaLusa.v2.Dtos.Requests.Customers;
using FacturaLusa.v2.Dtos.Requests.Items;
using FacturaLusa.v2.Dtos.Requests.PaymentConditions;
using FacturaLusa.v2.Dtos.Requests.PaymentMethods;
using FacturaLusa.v2.Dtos.Requests.Sales;
using FacturaLusa.v2.Dtos.Requests.Series;
using FacturaLusa.v2.Dtos.Requests.Units;
using FacturaLusa.v2.Dtos.Requests.VatRates;
using FacturaLusa.v2.Dtos.Responses.Currencies;
using FacturaLusa.v2.Dtos.Responses.Customers;
using FacturaLusa.v2.Dtos.Responses.Items;
using FacturaLusa.v2.Dtos.Responses.PaymentConditions;
using FacturaLusa.v2.Dtos.Responses.PaymentMethods;
using FacturaLusa.v2.Dtos.Responses.Sales;
using FacturaLusa.v2.Dtos.Responses.Series;
using FacturaLusa.v2.Dtos.Responses.Units;
using FacturaLusa.v2.Dtos.Responses.VatRates;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2
{
    public class FacturaLusaService : IFacturaLusaService
    {
        public string AccountUuid { get; }

        private readonly IFacturaLusaApi api;
        private readonly string apiKey;

        public FacturaLusaService(string accountUuid, string apiKey, IFacturaLusaApi api)
        {
            AccountUuid = accountUuid;
            this.apiKey = apiKey;
            this.api = api;
        }

        public Task<CreateUnitResponse> CreateUnit(CreateUnitRequest request) => api.CreateUnit(apiKey, request);
        public Task<SearchUnitResponse> SearchUnit(SearchUnitRequest request) => api.SearchUnit(apiKey, request);

        public Task<SearchVatRateResponse> SearchVatRate(SearchVatRateRequest request) => api.SearchVatRate(apiKey, request);
        public Task<CreateVatRateResponse> CreateVatRate(CreateVatRateRequest request) => api.CreateVatRate(apiKey, request);

        public Task<SearchItemResponse> SearchItem(SearchItemRequest request) => api.SearchItem(apiKey, request);
        public Task<CreateItemResponse> CreateItem(CreateItemRequest request) => api.CreateItem(apiKey, request);

        public Task<SearchCurrencyResponse> SearchCurrency(SearchCurrencyRequest request) => api.SearchCurrency(apiKey, request);
        public Task<CreateCurrencyResponse> CreateCurrency(CreateCurrencyRequest request) => api.CreateCurrency(apiKey, request);

        public Task<SearchPaymentMethodResponse> SearchPaymentMethod(SearchPaymentMethodRequest request) => api.SearchPaymentMethod(apiKey, request);
        public Task<CreatePaymentMethodResponse> CreatePaymentMethod(CreatePaymentMethodRequest request) => api.CreatePaymentMethod(apiKey, request);

        public Task<SearchSerieResponse> SearchSerie(SearchSerieRequest request) => api.SearchSerie(apiKey, request);
        public Task<CreateSerieResponse> CreateSerie(CreateSerieRequest request) => api.CreateSerie(apiKey, request);
        public Task<CheckSerieCommunicationResponse> CheckSerieCommunication(CheckSerieCommunicationRequest request) => api.CheckSerieCommunication(apiKey, request);
        public Task<CommunicateSerieResponse> CommunicateSerie(CommunicateSerieRequest request) => api.CommunicateSerie(apiKey, request);

        public Task<SearchPaymentConditionResponse> SearchPaymentCondition(SearchPaymentConditionRequest request) => api.SearchPaymentCondition(apiKey, request);
        public Task<CreatePaymentConditionResponse> CreatePaymentCondition(CreatePaymentConditionRequest request) => api.CreatePaymentCondition(apiKey, request);

        public Task<SearchCustomerResponse> SearchCustomer(SearchCustomerRequest request) => api.SearchCustomer(apiKey, request);
        public Task<CreateCustomerResponse> CreateCustomer(CreateCustomerRequest request) => api.CreateCustomer(apiKey, request);

        public Task<SearchSaleResponse> SearchSale(SearchSaleRequest request) => api.SearchSale(apiKey, request);
        public Task<CreateSaleResponse> CreateSale(CreateSaleRequest request) => api.CreateSale(apiKey, request);
        public Task<DownloadSaleFileResponse> DownloadSaleFile(DownloadSaleFileRequest request) => api.DownloadSaleFile(apiKey, request);
        public Task<CancelSaleResponse> CancelSale(CancelSaleRequest request) => api.CancelSale(apiKey, request);
    }
}