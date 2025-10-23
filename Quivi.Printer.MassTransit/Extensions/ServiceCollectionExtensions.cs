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
            serviceCollection.AddSingleton<IPrinterRabbitMqSettings>(p => p.GetService<T>()!);
            serviceCollection.AddSingleton<MessageService>();
            serviceCollection.AddScoped<PrinterMessageConnector>();
            serviceCollection.AddScoped<IPrinterMessageConnector>(p => p.GetService<PrinterMessageConnector>()!);

            serviceCollection.AddSingleton<IBusControl>((p) =>
            {
                var settings = p.GetService<IPrinterRabbitMqSettings>()!;
                return Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    cfg.Host(settings.Host, (ushort)settings.Port, "/", h =>
                    {
                        h.Username(settings.Username);
                        h.Password(settings.Password);
                    });

                    cfg.ReceiveEndpoint("status", e => e.Consumer(() => p.GetService<MessageService>()));
                });
            });
            serviceCollection.AddSingleton<IBus>(p => p.GetService<IBusControl>()!);
            serviceCollection.AddSingleton<ISendEndpointProvider>(p => p.GetService<IBusControl>()!);
            serviceCollection.AddSingleton<IPublishEndpoint>(p => p.GetService<IBusControl>()!);

            serviceCollection.AddHostedService(p => p.GetService<MessageService>()!);
        }
    }
}