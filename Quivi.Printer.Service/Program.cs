using MassTransit;
using Quivi.Printer.Service.Configurations;
using Quivi.Printer.Service.Consumers;
using System.Net.NetworkInformation;

namespace Quivi.Printer.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            var config = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>()!;

            string id = GetMacAddress();
            builder.Services.AddMassTransit(configurator =>
            {
                configurator.AddConsumer<PrintMessageConsumer>();
                configurator.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(config.Host, config.Port, config.VirtualHost, h =>
                    {
                        h.Username(config.Username);
                        h.Password(config.Password);
                    });

                    cfg.ReceiveEndpoint($"printers.{id}", e =>
                    {
                        e.ConfigureConsumer<PrintMessageConsumer>(context);
                    });
                });
            });

            builder.Services.Configure<HostOptions>(opts =>
            {
                opts.ShutdownTimeout = TimeSpan.FromSeconds(30);
            });

            builder.Services.AddWindowsService();

            var host = builder.Build();
            host.Run();
        }

        public static string GetMacAddress()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                                    .Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
                                                  nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                                    .Select(nic => nic.GetPhysicalAddress().ToString())
                                    .FirstOrDefault() ?? "UNKNOWN";
        }
    }
}