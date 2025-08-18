using FacturaLusa.v2.Dtos.Responses.Sales;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Commands
{
    public abstract class AGetOrCreateSaleCommandHandler
    {
        protected readonly IFacturaLusaCacheProvider CacheProvider;

        public AGetOrCreateSaleCommandHandler(IFacturaLusaCacheProvider cacheProvider)
        {
            CacheProvider = cacheProvider;
        }

        protected virtual async Task PostSaleCreation(IFacturaLusaService service, CreateSaleResponse createdSale)
        {
            // Store in the cache to reduce later calls
            await CacheProvider.GetOrCreateSaleId(service.AccountUuid, createdSale.DocumentFullNumber, () => Task.FromResult(createdSale.Id), TimeSpan.FromDays(30));

            if (string.IsNullOrWhiteSpace(createdSale.UrlFile) == false)
                await CacheProvider.GetOrCreateSaleDocumentUrl(service.AccountUuid, createdSale.Id, createdSale.FileFormat, () => Task.FromResult(createdSale.UrlFile), TimeSpan.FromDays(30));
        }
    }
}