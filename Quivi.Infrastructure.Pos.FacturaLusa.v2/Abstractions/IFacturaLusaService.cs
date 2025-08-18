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

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions
{
    public interface IFacturaLusaService
    {
        string AccountUuid { get; }

        #region Units
        Task<SearchUnitResponse> SearchUnit(SearchUnitRequest request);
        Task<CreateUnitResponse> CreateUnit(CreateUnitRequest request);
        #endregion

        #region Vat Rates
        Task<SearchVatRateResponse> SearchVatRate(SearchVatRateRequest request);
        Task<CreateVatRateResponse> CreateVatRate(CreateVatRateRequest request);
        #endregion

        #region Items
        Task<SearchItemResponse> SearchItem(SearchItemRequest request);
        Task<CreateItemResponse> CreateItem(CreateItemRequest request);
        #endregion

        #region Currency
        Task<SearchCurrencyResponse> SearchCurrency(SearchCurrencyRequest request);
        Task<CreateCurrencyResponse> CreateCurrency(CreateCurrencyRequest request);
        #endregion


        #region Payment Methods
        Task<SearchPaymentMethodResponse> SearchPaymentMethod(SearchPaymentMethodRequest request);
        Task<CreatePaymentMethodResponse> CreatePaymentMethod(CreatePaymentMethodRequest request);
        #endregion

        #region Series
        Task<SearchSerieResponse> SearchSerie(SearchSerieRequest request);
        Task<CreateSerieResponse> CreateSerie(CreateSerieRequest request);
        Task<CheckSerieCommunicationResponse> CheckSerieCommunication(CheckSerieCommunicationRequest request);
        Task<CommunicateSerieResponse> CommunicateSerie(CommunicateSerieRequest request);
        #endregion

        #region Payment Conditions
        Task<SearchPaymentConditionResponse> SearchPaymentCondition(SearchPaymentConditionRequest request);
        Task<CreatePaymentConditionResponse> CreatePaymentCondition(CreatePaymentConditionRequest request);
        #endregion

        #region Customers
        Task<SearchCustomerResponse> SearchCustomer(SearchCustomerRequest request);
        Task<CreateCustomerResponse> CreateCustomer(CreateCustomerRequest request);
        #endregion

        #region Sales
        Task<SearchSaleResponse> SearchSale(SearchSaleRequest request);
        Task<CreateSaleResponse> CreateSale(CreateSaleRequest request);
        Task<DownloadSaleFileResponse> DownloadSaleFile(DownloadSaleFileRequest request);
        Task<CancelSaleResponse> CancelSale(CancelSaleRequest request);
        #endregion
    }
}