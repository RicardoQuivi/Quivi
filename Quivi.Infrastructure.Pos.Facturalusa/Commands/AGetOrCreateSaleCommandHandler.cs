using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Sales;

namespace Quivi.Infrastructure.Pos.Facturalusa.Commands
{
    public abstract class AGetOrCreateSaleCommandHandler
    {
        protected readonly IFacturalusaCacheProvider CacheProvider;

        public AGetOrCreateSaleCommandHandler(IFacturalusaCacheProvider cacheProvider)
        {
            CacheProvider = cacheProvider;
        }

        protected virtual async Task PostSaleCreation(IFacturalusaService service, CreateSaleResponse createdSale)
        {
            // Store in the cache to reduce later calls
            await CacheProvider.GetOrCreateSaleId(service.AccountUuid, createdSale.Data.ComposedNumber ?? string.Empty, () => Task.FromResult(createdSale.Data.Id), TimeSpan.FromDays(30));

            if (string.IsNullOrWhiteSpace(createdSale.Data.PdfFileUrl) == false)
            {
                await CacheProvider.GetOrCreateSaleDocumentUrl(service.AccountUuid, createdSale.Data.Id, createdSale.Data.PdfFileFormat, () => Task.FromResult(createdSale.Data.PdfFileUrl), TimeSpan.FromDays(30));
            }
        }
    }
}