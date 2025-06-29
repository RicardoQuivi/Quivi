using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Quivi.Infrastructure.Abstractions.Services.Printing;
using Quivi.Printer.MassTransit.Configurations;

namespace Quivi.Printer.MassTransit.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigurePrinterConnector<T>(this IServiceCollection serviceCollection) where T : IPrinterRabbitMqSettings
        {
            serviceCollection.AddSingleton<PrinterMessageConnector>();
            serviceCollection.AddScoped<IPrinterMessageConnector>(p => p.GetService<PrinterMessageConnector>()!);

            serviceCollection.AddScoped<IBusControl>((p) =>
            {
                var settings = p.GetService<T>()!;
                return Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    cfg.Host(settings.Host, (ushort)settings.Port, "/", h =>
                    {
                        h.Username(settings.Username);
                        h.Password(settings.Password);
                    });

                    cfg.ReceiveEndpoint("status", e =>
                    {
                        e.Consumer(() => p.GetService<PrinterMessageConnector>());
                    });
                });
            });
            serviceCollection.AddScoped<IBus>(p => p.GetService<IBusControl>()!);

            serviceCollection.AddScoped<ISendEndpointProvider>(p => p.GetService<IBusControl>()!);
            serviceCollection.AddScoped<IPublishEndpoint>(p => p.GetService<IBusControl>()!);
        }
    }
}
