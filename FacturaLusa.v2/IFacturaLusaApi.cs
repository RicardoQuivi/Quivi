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

namespace FacturaLusa.v2
{
    public interface IFacturaLusaApi
    {
        #region Units
        Task<SearchUnitResponse> SearchUnit(string apiKey, SearchUnitRequest request);
        Task<CreateUnitResponse> CreateUnit(string apiKey, CreateUnitRequest request);
        #endregion

        #region Vat Rates
        Task<SearchVatRateResponse> SearchVatRate(string apiKey, SearchVatRateRequest request);
        Task<CreateVatRateResponse> CreateVatRate(string apiKey, CreateVatRateRequest request);
        #endregion

        #region Items
        Task<SearchItemResponse> SearchItem(string apiKey, SearchItemRequest request);
        Task<CreateItemResponse> CreateItem(string apiKey, CreateItemRequest request);
        #endregion

        #region Currency
        Task<SearchCurrencyResponse> SearchCurrency(string apiKey, SearchCurrencyRequest request);
        Task<CreateCurrencyResponse> CreateCurrency(string apiKey, CreateCurrencyRequest request);
        #endregion

        #region Payment Methods
        Task<SearchPaymentMethodResponse> SearchPaymentMethod(string apiKey, SearchPaymentMethodRequest request);
        Task<CreatePaymentMethodResponse> CreatePaymentMethod(string apiKey, CreatePaymentMethodRequest request);
        #endregion

        #region Series
        Task<SearchSerieResponse> SearchSerie(string apiKey, SearchSerieRequest request);
        Task<CreateSerieResponse> CreateSerie(string apiKey, CreateSerieRequest request);
        Task<CheckSerieCommunicationResponse> CheckSerieCommunication(string apiKey, CheckSerieCommunicationRequest request);
        Task<CommunicateSerieResponse> CommunicateSerie(string apiKey, CommunicateSerieRequest request);
        #endregion

        #region Payment Conditions
        Task<SearchPaymentConditionResponse> SearchPaymentCondition(string apiKey, SearchPaymentConditionRequest request);
        Task<CreatePaymentConditionResponse> CreatePaymentCondition(string apiKey, CreatePaymentConditionRequest request);
        #endregion

        #region Customers
        Task<SearchCustomerResponse> SearchCustomer(string apiKey, SearchCustomerRequest request);
        Task<CreateCustomerResponse> CreateCustomer(string apiKey, CreateCustomerRequest request);
        #endregion

        #region Sales
        Task<SearchSaleResponse> SearchSale(string apiKey, SearchSaleRequest request);
        Task<CreateSaleResponse> CreateSale(string apiKey, CreateSaleRequest request);
        Task<DownloadSaleFileResponse> DownloadSaleFile(string apiKey, DownloadSaleFileRequest request);
        Task<CancelSaleResponse> CancelSale(string apiKey, CancelSaleRequest request);
        #endregion
    }
}