using Quivi.Infrastructure.Pos.Facturalusa.Models.Currencies;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Customers;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Items;
using Quivi.Infrastructure.Pos.Facturalusa.Models.PaymentConditions;
using Quivi.Infrastructure.Pos.Facturalusa.Models.PaymentMethods;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Sales;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Series;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Units;
using Quivi.Infrastructure.Pos.Facturalusa.Models.VatRates;

namespace Quivi.Infrastructure.Pos.Facturalusa.Abstractions
{
    public interface IFacturalusaService
    {
        /// <summary>
        /// The unique ID of Facturalusa account.
        /// </summary>
        string AccountUuid { get; }

        #region Customers

        /// <summary>
        /// Find customers by criteria.
        /// </summary>
        /// <param name="request">The request criteria.</param>
        /// <returns>The customers that match the criteria.</returns>
        Task<GetCustomersResponse> GetCustomers(GetCustomersRequest request);

        /// <summary>
        /// Creates a new Customer.
        /// </summary>
        /// <param name="request">The customer data.</param>
        /// <returns>The created customer with the generated ID.</returns>
        Task<CreateCustomerResponse> CreateCustomer(CreateCustomerRequest request);

        #endregion

        #region Items

        /// <summary>
        /// Find Items by criteria.
        /// </summary>
        /// <param name="request">The criteria.</param>
        /// <returns>The items that matched the criteria.</returns>
        Task<GetItemsResponse> GetItems(GetItemsRequest request);

        /// <summary>
        /// Creates a new Item.
        /// </summary>
        /// <param name="request">The Item data.</param>
        /// <returns>The created Item with the generated ID.</returns>
        Task<CreateItemResponse> CreateItem(CreateItemRequest request);

        #endregion

        #region Series

        /// <summary>
        /// Find Series by criteria.
        /// </summary>
        /// <param name="request">The criteria.</param>
        /// <returns>The Series that matched the criteria.</returns>
        Task<GetSeriesResponse> GetSeries(GetSeriesRequest request);

        /// <summary>
        /// Creates a new Serie.
        /// </summary>
        /// <param name="request">The Serie data.</param>
        /// <returns>The created Serie with the generated ID.</returns>
        Task<CreateSerieResponse> CreateSerie(CreateSerieRequest request);

        /// <summary>
        /// Communicates a Serie to AT.
        /// </summary>
        /// <returns></returns>
        Task<CommunicateSerieResponse> CommunicateSerie(long serieId);

        /// <summary>
        /// Check if a Serie was already communicated to AT for a specific document type.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<CheckCommunicateSerieResponse> CheckCommunicateSerie(long serieId, CheckCommunicateSerieRequest request);

        #endregion

        #region Units

        /// <summary>
        /// Find Units by criteria.
        /// </summary>
        /// <param name="request">The criteria.</param>
        /// <returns>The Units that matched the criteria.</returns>
        Task<GetUnitsResponse> GetUnits(GetUnitsRequest request);

        /// <summary>
        /// Creates a new Unit.
        /// </summary>
        /// <param name="request">The Unit data.</param>
        /// <returns>The created Unit with the generated ID.</returns>
        Task<CreateUnitResponse> CreateUnit(CreateUnitRequest request);

        #endregion

        #region Sales

        /// <summary>
        /// Find Sales by criteria.
        /// </summary>
        /// <param name="request">The criteria.</param>
        /// <returns>The Sales that matched the criteria.</returns>
        Task<GetSalesResponse> GetSale(GetSalesRequest request);
        
        /// <summary>
        /// Download a Sale entity file.
        /// </summary>
        /// <param name="saleId">The sale ID.</param>
        /// <param name="request">Empty request.</param>
        /// <returns>The file URL.</returns>
        Task<DownloadSaleResponse> DownloadSale(long saleId, DownloadSaleRequest request);

        /// <summary>
        /// Creates a new Sale.
        /// </summary>
        /// <param name="request">The Sale data.</param>
        /// <returns>The created Sale with the generated ID.</returns>
        Task<CreateSaleResponse> CreateSale(CreateSaleRequest request);

        /// <summary>
        /// Cancels a new Sale.
        /// </summary>
        /// <param name="request">The Sale data.</param>
        /// <returns>The created Sale with the generated ID.</returns>
        Task<CancelSaleResponse> CancelSale(long saleId, CancelSaleRequest request);

        #endregion

        #region Payment Methods

        /// <summary>
        /// Find PaymentMethods by criteria.
        /// </summary>
        /// <param name="request">The criteria.</param>
        /// <returns>The PaymentMethods that matched the criteria.</returns>
        Task<GetPaymentMethodsResponse> GetPaymentMethods(GetPaymentMethodsRequest request);

        /// <summary>
        /// Creates a new PaymentMethod.
        /// </summary>
        /// <param name="request">The PaymentMethod data.</param>
        /// <returns>The created PaymentMethod with the generated ID.</returns>
        Task<CreatePaymentMethodResponse> CreatePaymentMethod(CreatePaymentMethodRequest request);

        #endregion

        #region Currencies

        /// <summary>
        /// Find Currencies by criteria.
        /// </summary>
        /// <param name="request">The criteria.</param>
        /// <returns>The Currencies that matched the criteria.</returns>
        Task<GetCurrenciesResponse> GetCurrencies(GetCurrenciesRequest request);

        /// <summary>
        /// Creates a new Currency.
        /// </summary>
        /// <param name="request">The Currency data.</param>
        /// <returns>The created Currency with the generated ID.</returns>
        Task<CreateCurrencyResponse> CreateCurrency(CreateCurrencyRequest request);

        #endregion

        #region VAT Rates

        /// <summary>
        /// Find VatRates by criteria.
        /// </summary>
        /// <param name="request">The criteria.</param>
        /// <returns>The VatRates that matched the criteria.</returns>
        Task<GetVatRatesResponse> GetVatRates(GetVatRatesRequest request);

        /// <summary>
        /// Creates a new VatRate.
        /// </summary>
        /// <param name="request">The VatRate data.</param>
        /// <returns>The created VatRate with the generated ID.</returns>
        Task<CreateVatRateResponse> CreateVatRate(CreateVatRateRequest request);

        #endregion

        #region Payment Conditions

        /// <summary>
        /// Find PaymentConditions by criteria.
        /// </summary>
        /// <param name="request">The criteria.</param>
        /// <returns>The PaymentConditions that matched the criteria.</returns>
        Task<GetPaymentConditionsResponse> GetPaymentConditions(GetPaymentConditionsRequest request);

        /// <summary>
        /// Creates a new PaymentCondition.
        /// </summary>
        /// <param name="request">The PaymentCondition data.</param>
        /// <returns>The created PaymentCondition with the generated ID.</returns>
        Task<CreatePaymentConditionResponse> CreatePaymentCondition(CreatePaymentConditionRequest request);

        #endregion
    }
}
