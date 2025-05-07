using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Currencies;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Customers;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Items;
using Quivi.Infrastructure.Pos.Facturalusa.Models.PaymentConditions;
using Quivi.Infrastructure.Pos.Facturalusa.Models.PaymentMethods;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Sales;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Series;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Units;
using Quivi.Infrastructure.Pos.Facturalusa.Models.VatRates;
using Refit;
using System.Net;

namespace Quivi.Infrastructure.Pos.Facturalusa
{
    internal class FacturalusaService : IFacturalusaService
    {
        private readonly Lazy<IFacturalusaApi> _facturalusaApi;

        public FacturalusaService(string accountUuid, string baseUrl, HttpFacturalusaHandler handler)
        {
            AccountUuid = accountUuid;
            _facturalusaApi = new Lazy<IFacturalusaApi>(() =>
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                var httpClient = RestService.CreateHttpClient(baseUrl, new RefitSettings
                {
                    HttpMessageHandlerFactory = () => handler,
                });
                return RestService.For<IFacturalusaApi>(httpClient);
            });
        }

        public string AccountUuid { get; }

        public Task<CheckCommunicateSerieResponse> CheckCommunicateSerie(long serieId, CheckCommunicateSerieRequest request) => _facturalusaApi.Value.CheckCommunicateSerie(serieId, request);

        public Task<CommunicateSerieResponse> CommunicateSerie(long serieId) => _facturalusaApi.Value.CommunicateSerie(serieId, new CommunicateSerieRequest());

        public Task<CreateCurrencyResponse> CreateCurrency(CreateCurrencyRequest request) => _facturalusaApi.Value.CreateCurrency(request);

        public Task<CreateCustomerResponse> CreateCustomer(CreateCustomerRequest request) => _facturalusaApi.Value.CreateCustomer(request);

        public Task<CreateItemResponse> CreateItem(CreateItemRequest request) => _facturalusaApi.Value.CreateItem(request);

        public Task<CreatePaymentConditionResponse> CreatePaymentCondition(CreatePaymentConditionRequest request) => _facturalusaApi.Value.CreatePaymentCondition(request);

        public Task<CreatePaymentMethodResponse> CreatePaymentMethod(CreatePaymentMethodRequest request) => _facturalusaApi.Value.CreatePaymentMethod(request);

        public Task<CreateSaleResponse> CreateSale(CreateSaleRequest request) => _facturalusaApi.Value.CreateSale(request);
       
        public Task<CancelSaleResponse> CancelSale(long saleId, CancelSaleRequest request) => _facturalusaApi.Value.CancelSale(saleId, request);

        public Task<CreateSerieResponse> CreateSerie(CreateSerieRequest request) => _facturalusaApi.Value.CreateSerie(request);

        public Task<CreateUnitResponse> CreateUnit(CreateUnitRequest request) => _facturalusaApi.Value.CreateUnit(request);

        public Task<CreateVatRateResponse> CreateVatRate(CreateVatRateRequest request) => _facturalusaApi.Value.CreateVatRate(request);

        public Task<DownloadSaleResponse> DownloadSale(long saleId, DownloadSaleRequest request) => _facturalusaApi.Value.DownloadSale(saleId, request);

        public Task<GetCurrenciesResponse> GetCurrencies(GetCurrenciesRequest request) => _facturalusaApi.Value.GetCurrencies(request);

        public Task<GetCustomersResponse> GetCustomers(GetCustomersRequest request) => _facturalusaApi.Value.GetCustomers(request);

        public Task<GetItemsResponse> GetItems(GetItemsRequest request) => _facturalusaApi.Value.GetItems(request);

        public Task<GetPaymentConditionsResponse> GetPaymentConditions(GetPaymentConditionsRequest request) => _facturalusaApi.Value.GetPaymentConditions(request);

        public Task<GetPaymentMethodsResponse> GetPaymentMethods(GetPaymentMethodsRequest request) => _facturalusaApi.Value.GetPaymentMethods(request);

        public Task<GetSalesResponse> GetSale(GetSalesRequest request) => _facturalusaApi.Value.GetSale(request);

        public Task<GetSeriesResponse> GetSeries(GetSeriesRequest request) => _facturalusaApi.Value.GetSeries(request);

        public Task<GetUnitsResponse> GetUnits(GetUnitsRequest request) => _facturalusaApi.Value.GetUnits(request);

        public Task<GetVatRatesResponse> GetVatRates(GetVatRatesRequest request) => _facturalusaApi.Value.GetVatRates(request);

        public Task<UpdateItemResponse> UpdateItem(long itemId,UpdateItemRequest request) => _facturalusaApi.Value.UpdateItem(itemId, request);
    }
}