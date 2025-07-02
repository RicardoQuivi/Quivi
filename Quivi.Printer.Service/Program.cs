using MassTransit;
using Quivi.Printer.Service.Configurations;
using Quivi.Printer.Service.Consumers;

namespace Quivi.Printer.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            var config = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>()!;

            string id = DeviceIdManager.GetOrCreateDeviceId();
            builder.Services.AddSingleton<PrintMessageConsumer>();
            builder.Services.AddSingleton<IBusControl>((p) =>
            {
                return Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    cfg.Host(config.Host, (ushort)config.Port, "/", h =>
                    {
                        h.Username(config.Username);
                        h.Password(config.Password);
                    });

                    cfg.ReceiveEndpoint($"printers.{id}", e =>
                    {
                        e.Consumer(() => p.GetService<PrintMessageConsumer>());
                    });
                });
            });
            builder.Services.AddSingleton<IBus>(p => p.GetService<IBusControl>()!);
            builder.Services.AddSingleton<ISendEndpointProvider>(p => p.GetService<IBusControl>()!);
            builder.Services.AddSingleton<IPublishEndpoint>(p => p.GetService<IBusControl>()!);


            builder.Services.Configure<HostOptions>(opts =>
            {
                opts.ShutdownTimeout = TimeSpan.FromSeconds(30);
            });

            builder.Services.AddWindowsService();
            builder.Services.AddHostedService<MessageService>();

            var host = builder.Build();
            host.Run();
        }
    }
}