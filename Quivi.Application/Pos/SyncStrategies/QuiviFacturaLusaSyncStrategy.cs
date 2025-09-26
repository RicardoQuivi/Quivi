using Quivi.Application.Queries.MenuItems;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Pos.Invoicing;
using Quivi.Infrastructure.Abstractions.Pos.Invoicing.Models;

namespace Quivi.Application.Pos.SyncStrategies
{
    public class QuiviFacturaLusaSyncStrategy : AQuiviSyncStrategy<QuiviFacturalusaSyncSettings>
    {
        private readonly IIdConverter idConverter;

        public QuiviFacturaLusaSyncStrategy(ICommandProcessor commandProcessor,
                                            IInvoiceGatewayFactory invoiceGatewayFactory,
                                            IQueryProcessor queryProcessor,
                                            IIdConverter idConverter) : base(commandProcessor, invoiceGatewayFactory, queryProcessor)
        {
            this.idConverter = idConverter;
        }

        public override IntegrationType IntegrationType => IntegrationType.QuiviViaFacturalusa;

        public override bool ImplementsRefundChargeAsCancellation => true;

        public override QuiviFacturalusaSyncSettings ParseSyncSettings(PosIntegration configuration) => new QuiviFacturalusaSyncSettings(configuration);

        public override async Task SyncMenu(PosIntegration configuration, IEnumerable<int>? menuItemIds = null)
        {
            var settings = ParseSyncSettings(configuration);
            if (settings.SkipInvoice)
                return;

            var menuItemsQuery = await QueryProcessor.Execute(new GetMenuItemsAsyncQuery
            {
                Ids = menuItemIds,
                MerchantIds = [configuration.MerchantId],
                PageSize = null,
            });

            var invoiceGateway = InvoiceGatewayFactory.GetInvoiceGateway(settings);
            await invoiceGateway.UpsertInvoiceItems(menuItemsQuery.Select(m => new ProductItem(InvoiceItemType.ProcessedProducts)
            {
                CorrelationId = idConverter.ToPublicId(m.Id),
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                TaxPercentage = m.VatRate,
                IsDeleted = m.DeletedDate.HasValue,
            }));
        }
    }
}