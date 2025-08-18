using FacturaLusa.v2.Dtos;
using Quivi.Infrastructure.Abstractions.Services;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2
{
    public class FacturaLusaCacheProvider : IFacturaLusaCacheProvider
    {
        private readonly ICacheProvider _cacheProvider;

        public FacturaLusaCacheProvider(ICacheProvider cacheProvider)
        {
            _cacheProvider = cacheProvider;
        }

        private string GetContext(string accountUuid) => $"FL#{accountUuid.Trim()}";

        public async Task<long> GetOrCreateSaleId(string accountUuid, string documentId, Func<Task<long>> builderFunc, TimeSpan expiration)
        {
            string key = $"DocIdToSaleId#{documentId.Trim()}";
            return await _cacheProvider.GetOrCreate(
                key,
                () => builderFunc(),
                GetContext(accountUuid),
                expiration,
                extendExpiration: true
            );
        }

        public async Task<string> GetOrCreateSaleDocumentUrl(string accountUuid, long saleId, DocumentFormat format, Func<Task<string>> builderFunc, TimeSpan expiration)
        {
            string key = $"SaleIdDocumentUrl#{saleId}#{format}";
            return (await _cacheProvider.GetOrCreate(
                key,
                () => builderFunc(),
                GetContext(accountUuid),
                expiration,
                extendExpiration: true
            ))!;
        }

        public async Task<CacheResult<bool>> GetIsCommunicatedSerie(string accountUuid, long serieId, DocumentType docType)
        {
            string key = $"SerieIsCommunicated#{serieId}#{docType}";
            return await _cacheProvider.Get<bool>(key, GetContext(accountUuid), true);
        }

        public async Task<bool> CreateIsCommunicatedSerie(string accountUuid, long serieId, DocumentType docType, bool isCommunicated, TimeSpan expiration)
        {
            string key = $"SerieIsCommunicated#{serieId}#{docType}";
            return await _cacheProvider.Set(key, isCommunicated, GetContext(accountUuid), expiration);
        }

        public async Task<Currency> GetOrCreateCurrency(string accountUuid, string isoCode, Func<Task<Currency>> builderFunc, TimeSpan expiration)
        {
            string key = $"Currency#{isoCode.ToUpperInvariant().Trim()}";
            return (await _cacheProvider.GetOrCreate(
                key,
                () => builderFunc(),
                GetContext(accountUuid),
                expiration,
                extendExpiration: true
            ))!;
        }

        public async Task<Customer> GetOrCreateCustomer(string accountUuid, string vatNumber, Func<Task<Customer>> builderFunc, TimeSpan expiration)
        {
            string key = $"Customer#{vatNumber?.Trim()}";
            return (await _cacheProvider.GetOrCreate(
                key,
                () => builderFunc(),
                GetContext(accountUuid),
                expiration,
                extendExpiration: true
            ))!;
        }

        public async Task<Item> GetOrCreateItem(string accountUuid, string reference, Func<Task<Item>> builderFunc, TimeSpan expiration, bool overrideEntry)
        {
            string key = $"Item#{reference.Trim()}";

            if (overrideEntry)
            {
                var entry = await builderFunc();
                await _cacheProvider.Set(key, entry, GetContext(accountUuid), expiration);
                return entry;
            }

            return (await _cacheProvider.GetOrCreate(
                key,
                () => builderFunc(),
                GetContext(accountUuid),
                expiration,
                extendExpiration: true
            ))!;
        }

        public async Task<PaymentCondition> GetOrCreatePaymentCondition(string accountUuid, string name, Func<Task<PaymentCondition>> builderFunc, TimeSpan expiration)
        {
            string key = $"PaymentCondition#{name.ToUpperInvariant().Trim()}";
            return (await _cacheProvider.GetOrCreate(
                key,
                () => builderFunc(),
                GetContext(accountUuid),
                expiration,
                extendExpiration: true
            ))!;
        }

        public async Task<PaymentMethod> GetOrCreatePaymentMethod(string accountUuid, string name, Func<Task<PaymentMethod>> builderFunc, TimeSpan expiration)
        {
            string key = $"PaymentMethod#{name.ToUpperInvariant().Trim()}";
            return (await _cacheProvider.GetOrCreate(
                key,
                () => builderFunc(),
                GetContext(accountUuid),
                expiration,
                extendExpiration: true
            ))!;
        }

        public async Task<Serie> GetOrCreateSerie(string accountUuid, string name, Func<Task<Serie>> builderFunc, TimeSpan expiration)
        {
            string key = $"Serie#{name.ToUpperInvariant().Trim()}";
            return (await _cacheProvider.GetOrCreate(
                key,
                () => builderFunc(),
                GetContext(accountUuid),
                expiration,
                extendExpiration: true
            ))!;
        }

        public async Task<Unit> GetOrCreateUnit(string accountUuid, string name, Func<Task<Unit>> builderFunc, TimeSpan expiration)
        {
            string key = $"Unit#{name.ToUpperInvariant().Trim()}";
            return (await _cacheProvider.GetOrCreate(
                key,
                () => builderFunc(),
                GetContext(accountUuid),
                expiration,
                extendExpiration: true
            ))!;
        }

        public async Task<VatRate> GetOrCreateVatRate(string accountUuid, decimal percentageValue, Func<Task<VatRate>> builderFunc, TimeSpan expiration)
        {
            string key = $"VatRate#{percentageValue}";
            return (await _cacheProvider.GetOrCreate(
                key,
                () => builderFunc(),
                GetContext(accountUuid),
                expiration,
                extendExpiration: true
            ))!;
        }
    }
}
