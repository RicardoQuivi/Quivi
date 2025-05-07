using Quivi.Infrastructure.Abstractions.Services;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Currencies;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Customers;
using Quivi.Infrastructure.Pos.Facturalusa.Models.DocumentTypes;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Items;
using Quivi.Infrastructure.Pos.Facturalusa.Models.PaymentConditions;
using Quivi.Infrastructure.Pos.Facturalusa.Models.PaymentMethods;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Sales;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Series;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Units;
using Quivi.Infrastructure.Pos.Facturalusa.Models.VatRates;

namespace Quivi.Infrastructure.Pos.Facturalusa
{
    public interface IFacturalusaCacheProvider
    {
        Task<long> GetOrCreateSaleId(string accountUuid, string documentId, Func<Task<long>> builderFunc, TimeSpan expiration);

        Task<string> GetOrCreateSaleDocumentUrl(string accountUuid, long saleId, DownloadSaleFormat format, Func<Task<string>> builderFunc, TimeSpan expiration);

        Task<CacheResult<bool>> GetIsCommunicatedSerie(string accountUuid, long serieId, DocumentType docType);
        
        Task<bool> CreateIsCommunicatedSerie(string accountUuid, long serieId, DocumentType docType, bool isCommunicated, TimeSpan expiration);

        Task<Currency> GetOrCreateCurrency(string accountUuid, string isoCode, Func<Task<Currency>> builderFunc, TimeSpan expiration);

        Task<ReadOnlyCustomer> GetOrCreateCustomer(string accountUuid, string vatNumber, Func<Task<ReadOnlyCustomer>> builderFunc, TimeSpan expiration);

        Task<ReadonlyItem> GetOrCreateItem(string accountUuid, string reference, Func<Task<ReadonlyItem>> builderFunc, TimeSpan expiration, bool overrideEntry = false);

        Task<PaymentCondition> GetOrCreatePaymentCondition(string accountUuid, string name, Func<Task<PaymentCondition>> builderFunc, TimeSpan expiration);

        Task<ReadonlyPaymentMethod> GetOrCreatePaymentMethod(string accountUuid, string name, Func<Task<ReadonlyPaymentMethod>> builderFunc, TimeSpan expiration);

        Task<ReadonlySerie> GetOrCreateSerie(string accountUuid, string name, Func<Task<ReadonlySerie>> builderFunc, TimeSpan expiration);

        Task<Unit> GetOrCreateUnit(string accountUuid, string name, Func<Task<Unit>> builderFunc, TimeSpan expiration);

        Task<VatRate> GetOrCreateVatRate(string accountUuid, decimal percentageValue, Func<Task<VatRate>> builderFunc, TimeSpan expiration);
    }
}