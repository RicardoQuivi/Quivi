using Quivi.Application.Pos.SyncStrategies;
using Quivi.Infrastructure.Abstractions.Pos;
using Quivi.Infrastructure.Abstractions.Pos.Invoicing;
using Quivi.Infrastructure.Pos.Facturalusa;
using System.Collections.Concurrent;

namespace Quivi.Application.Pos.Invoicing
{
    public class InvoiceGatewayFactory : IInvoiceGatewayFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Lazy<IInvoiceGateway> _defaultGateway;
        private readonly ConcurrentDictionary<string, IInvoiceGateway> _loadedFacturalusaGateways = new ConcurrentDictionary<string, IInvoiceGateway>();

        public InvoiceGatewayFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _defaultGateway = new Lazy<IInvoiceGateway>(() => CreateDefaultGateway());
        }

        public IInvoiceGateway GetDefaultInvoiceGateway() => _defaultGateway.Value;

        private IInvoiceGateway CreateDefaultGateway()
        {
            throw new NotSupportedException("Missing default Invoice Gateway. Verify if the web.config has DeGrazieInvoiceGateway configured.");
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
            if (_loadedFacturalusaGateways.TryGetValue(settings.AccessToken, out var gateway) == false)
            {
                gateway = new FacturalusaGateway(_serviceProvider, settings.AccessToken, settings.MerchantId.ToString());
                _loadedFacturalusaGateways.TryAdd(settings.AccessToken, gateway);
            }

            return gateway;
        }
    }
}
