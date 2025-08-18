using FacturaLusa.v2.Dtos;
using Quivi.Infrastructure.Abstractions.Services;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions
{
    public interface IFacturaLusaCacheProvider
    {
        Task<long> GetOrCreateSaleId(string accountUuid, string documentId, Func<Task<long>> builderFunc, TimeSpan expiration);
        Task<string> GetOrCreateSaleDocumentUrl(string accountUuid, long saleId, DocumentFormat format, Func<Task<string>> builderFunc, TimeSpan expiration);
        Task<VatRate> GetOrCreateVatRate(string accountUuid, decimal percentageValue, Func<Task<VatRate>> builderFunc, TimeSpan expiration);
        Task<Unit> GetOrCreateUnit(string accountUuid, string name, Func<Task<Unit>> builderFunc, TimeSpan expiration);
        Task<Item> GetOrCreateItem(string accountUuid, string reference, Func<Task<Item>> builderFunc, TimeSpan expiration, bool overrideEntry);
        Task<Currency> GetOrCreateCurrency(string accountUuid, string isoCode, Func<Task<Currency>> builderFunc, TimeSpan expiration);
        Task<PaymentMethod> GetOrCreatePaymentMethod(string accountUuid, string name, Func<Task<PaymentMethod>> builderFunc, TimeSpan expiration);
        Task<Serie> GetOrCreateSerie(string accountUuid, string name, Func<Task<Serie>> builderFunc, TimeSpan expiration);
        Task<CacheResult<bool>> GetIsCommunicatedSerie(string accountUuid, long serieId, DocumentType docType);
        Task<bool> CreateIsCommunicatedSerie(string accountUuid, long serieId, DocumentType docType, bool isCommunicated, TimeSpan expiration);
        Task<PaymentCondition> GetOrCreatePaymentCondition(string accountUuid, string name, Func<Task<PaymentCondition>> builderFunc, TimeSpan expiration);
        Task<Customer> GetOrCreateCustomer(string accountUuid, string vatNumber, Func<Task<Customer>> builderFunc, TimeSpan expiration);
    }
}