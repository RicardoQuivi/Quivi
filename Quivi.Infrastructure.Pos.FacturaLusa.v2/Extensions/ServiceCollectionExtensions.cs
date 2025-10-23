using FacturaLusa.v2;
using Microsoft.Extensions.DependencyInjection;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Services;
using Quivi.Infrastructure.Extensions;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions;
using Quivi.Infrastructure.Services;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterFacturalusa<T>(this IServiceCollection serviceCollection) where T : IFacturaLusaSettings
        {
            serviceCollection.RegisterSingleton<IFacturaLusaSettings>(p => p.GetService<T>()!);
            serviceCollection.RegisterSingleton<ICacheProvider, MemoryCacheProvider>();
            serviceCollection.RegisterSingleton<IFacturaLusaApi>(p =>
            {
                var settings = p.GetService<IFacturaLusaSettings>()!;
                return new FacturaLusaApi(settings.Host);
            });
            serviceCollection.RegisterSingleton<IFacturaLusaServiceFactory, FacturaLusaServiceFactory>();
            serviceCollection.RegisterSingleton<IFacturaLusaCacheProvider, FacturaLusaCacheProvider>();
            serviceCollection.RegisterScoped(p =>
            {
                var facturalusaSettings = p.GetService<IFacturaLusaSettings>()!;
                return new FacturaLusaInvoiceGateway(p.GetService<IFacturaLusaServiceFactory>()!, facturalusaSettings.AccessToken, "Default")
                {
                    CommandProcessor = p.GetService<ICommandProcessor>()!,
                    QueryProcessor = p.GetService<IQueryProcessor>()!
                };
            });
            return serviceCollection;
        }
    }
}