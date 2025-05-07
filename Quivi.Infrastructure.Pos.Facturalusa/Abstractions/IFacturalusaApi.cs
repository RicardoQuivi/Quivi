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

namespace Quivi.Infrastructure.Pos.Facturalusa.Abstractions
{
    public interface IFacturalusaApi
    {
        #region Customers

        /// <summary>
        /// Find customers by criteria.
        /// </summary>
        /// <param name="request">The request criteria.</param>
        /// <returns>The customers that match the criteria.</returns>
        [Post("/customers/find")]
        Task<GetCustomersResponse> GetCustomers([Body] GetCustomersRequest request);

        /// <summary>
        /// Creates a new Customer.
        /// </summary>
        /// <param name="request">The customer data.</param>
        /// <returns>The created customer with the generated ID.</returns>
        [Post("/customers/create")]
        Task<CreateCustomerResponse> CreateCustomer([Body] CreateCustomerRequest request);

        #endregion

        #region Items

        /// <summary>
        /// Find Items by criteria.
        /// </summary>
        /// <param name="request">The criteria.</param>
        /// <returns>The items that matched the criteria.</returns>
        [Post("/items/find")]
        Task<GetItemsResponse> GetItems([Body] GetItemsRequest request);

        /// <summary>
        /// Creates a new Item.
        /// </summary>
        /// <param name="request">The Item data.</param>
        /// <returns>The created Item with the generated ID.</returns>
        [Post("/items/create")]
        Task<CreateItemResponse> CreateItem([Body] CreateItemRequest request);

        /// <summary>
        /// Updates an existing Item.
        /// </summary>
        /// <param name="itemId">The Item ID.</param>
        /// <param name="request">The Item data to update.</param>
        /// <returns>Update Status.</returns>
        [Post("/items/{itemId}/update")]
        Task<UpdateItemResponse> UpdateItem(long itemId, [Body] UpdateItemRequest request);

        #endregion

        #region Series

        /// <summary>
        /// Find Series by criteria.
        /// </summary>
        /// <param name="request">The criteria.</param>
        /// <returns>The Series that matched the criteria.</returns>
        [Post("/administration/series/find")]
        Task<GetSeriesResponse> GetSeries([Body] GetSeriesRequest request);

        /// <summary>
        /// Creates a new Serie.
        /// </summary>
        /// <param name="request">The Serie data.</param>
        /// <returns>The created Serie with the generated ID.</returns>
        [Post("/administration/series/create")]
        Task<CreateSerieResponse> CreateSerie([Body] CreateSerieRequest request);

        /// <summary>
        /// Communicates a Serie to AT.
        /// </summary>
        /// <returns></returns>
        [Post("/administration/series/{serieId}/communicate")]
        Task<CommunicateSerieResponse> CommunicateSerie(long serieId, [Body] CommunicateSerieRequest request);

        /// <summary>
        /// Check if a Serie was already communicated to AT for a specific document type.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Post("/administration/series/{serieId}/check_communication")]
        Task<CheckCommunicateSerieResponse> CheckCommunicateSerie(long serieId, [Body] CheckCommunicateSerieRequest request);

        #endregion

        #region Units

        /// <summary>
        /// Find Units by criteria.
        /// </summary>
        /// <param name="request">The criteria.</param>
        /// <returns>The Units that matched the criteria.</returns>
        [Post("/administration/units/find")]
        Task<GetUnitsResponse> GetUnits([Body] GetUnitsRequest request);

        /// <summary>
        /// Creates a new Unit.
        /// </summary>
        /// <param name="request">The Unit data.</param>
        /// <returns>The created Unit with the generated ID.</returns>
        [Post("/administration/units/create")]
        Task<CreateUnitResponse> CreateUnit([Body] CreateUnitRequest request);

        #endregion

        #region Sales

        /// <summary>
        /// Find Sales by criteria.
        /// </summary>
        /// <param name="request">The criteria.</param>
        /// <returns>The Sales that matched the criteria.</returns>
        [Post("/sales/find")]
        Task<GetSalesResponse> GetSale([Body] GetSalesRequest request);

        /// <summary>
        /// Download a Sale entity file.
        /// </summary>
        /// <param name="saleId">The sale ID.</param>
        /// <param name="request">Empty request.</param>
        /// <returns>The file URL.</returns>
        [Post("/sales/{saleId}/download")]
        Task<DownloadSaleResponse> DownloadSale(long saleId, [Body] DownloadSaleRequest request);

        /// <summary>
        /// Creates a new Sale.
        /// </summary>
        /// <param name="request">The Sale data.</param>
        /// <returns>The created Sale with the generated ID.</returns>
        [Post("/sales/create")]
        Task<CreateSaleResponse> CreateSale([Body] CreateSaleRequest request);
        
        /// <summary>
        /// Cancels a Sale.
        /// </summary>
        /// <param name="request">The Sale data.</param>
        /// <returns>The created Sale with the generated ID.</returns>
        [Post("/sales/{saleId}/cancel")]
        Task<CancelSaleResponse> CancelSale(long saleId, [Body] CancelSaleRequest request);

        #endregion

        #region Payment Methods

        /// <summary>
        /// Find PaymentMethods by criteria.
        /// </summary>
        /// <param name="request">The criteria.</param>
        /// <returns>The PaymentMethods that matched the criteria.</returns>
        [Post("/administration/paymentmethods/find")]
        Task<GetPaymentMethodsResponse> GetPaymentMethods([Body] GetPaymentMethodsRequest request);

        /// <summary>
        /// Creates a new PaymentMethod.
        /// </summary>
        /// <param name="request">The PaymentMethod data.</param>
        /// <returns>The created PaymentMethod with the generated ID.</returns>
        [Post("/administration/paymentmethods/create")]
        Task<CreatePaymentMethodResponse> CreatePaymentMethod([Body] CreatePaymentMethodRequest request);

        #endregion

        #region Currencies

        /// <summary>
        /// Find Currencies by criteria.
        /// </summary>
        /// <param name="request">The criteria.</param>
        /// <returns>The Currencies that matched the criteria.</returns>
        [Post("/administration/currencies/find")]
        Task<GetCurrenciesResponse> GetCurrencies([Body] GetCurrenciesRequest request);

        /// <summary>
        /// Creates a new Currency.
        /// </summary>
        /// <param name="request">The Currency data.</param>
        /// <returns>The created Currency with the generated ID.</returns>
        [Post("/administration/currencies/create")]
        Task<CreateCurrencyResponse> CreateCurrency([Body] CreateCurrencyRequest request);

        #endregion

        #region VAT Rates

        /// <summary>
        /// Find VatRates by criteria.
        /// </summary>
        /// <param name="request">The criteria.</param>
        /// <returns>The VatRates that matched the criteria.</returns>
        [Post("/administration/vatrates/find")]
        Task<GetVatRatesResponse> GetVatRates([Body] GetVatRatesRequest request);

        /// <summary>
        /// Creates a new VatRate.
        /// </summary>
        /// <param name="request">The VatRate data.</param>
        /// <returns>The created VatRate with the generated ID.</returns>
        [Post("/administration/vatrates/create")]
        Task<CreateVatRateResponse> CreateVatRate([Body] CreateVatRateRequest request);

        #endregion

        #region Payment Conditions

        /// <summary>
        /// Find PaymentConditions by criteria.
        /// </summary>
        /// <param name="request">The criteria.</param>
        /// <returns>The PaymentConditions that matched the criteria.</returns>
        [Post("/administration/paymentconditions/find")]
        Task<GetPaymentConditionsResponse> GetPaymentConditions([Body] GetPaymentConditionsRequest request);

        /// <summary>
        /// Creates a new PaymentCondition.
        /// </summary>
        /// <param name="request">The PaymentCondition data.</param>
        /// <returns>The created PaymentCondition with the generated ID.</returns>
        [Post("/administration/paymentconditions/create")]
        Task<CreatePaymentConditionResponse> CreatePaymentCondition([Body] CreatePaymentConditionRequest request);

        #endregion
    }
}
