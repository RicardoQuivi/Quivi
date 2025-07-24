using Quivi.Application.Pos.SyncStrategies;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Pos;
using Quivi.Infrastructure.Abstractions.Pos.Invoicing;
using Quivi.Infrastructure.Pos.Facturalusa;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;
using System.Collections.Concurrent;

namespace Quivi.Application.Pos.Invoicing
{
    public class InvoiceGatewayFactory : IInvoiceGatewayFactory
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IFacturalusaServiceFactory facturalusaServiceFactory;
        private readonly ConcurrentDictionary<string, IInvoiceGateway> loadedFacturalusaGateways = new ConcurrentDictionary<string, IInvoiceGateway>();

        public InvoiceGatewayFactory(IQueryProcessor queryProcessor, ICommandProcessor commandProcessor, IFacturalusaServiceFactory facturalusaServiceFactory)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.facturalusaServiceFactory = facturalusaServiceFactory;
        }

        public IInvoiceGateway GetInvoiceGateway(ISyncSettings settings)
        {
            // Facturalusa
            if (settings is QuiviFacturalusaSyncSettings facturalusaSettings)
                return CreateFacturalusaGateway(facturalusaSettings);

            throw new NotSupportedException($"The settings of type {settings.GetType().FullName} is not supported to build an Invoice Gateway.");
        }

        private IInvoiceGateway CreateFacturalusaGateway(QuiviFacturalusaSyncSettings settings)
        {
            if (loadedFacturalusaGateways.TryGetValue(settings.AccessToken, out var gateway) == false)
            {
                gateway = new FacturalusaGateway(facturalusaServiceFactory, settings.AccessToken, settings.MerchantId.ToString())
                {
                    CommandProcessor = commandProcessor,
                    QueryProcessor = queryProcessor,
                };
                loadedFacturalusaGateways.TryAdd(settings.AccessToken, gateway);
            }

            return gateway;
        }
    }
}