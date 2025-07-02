using Hangfire;
using Quivi.Application.Extensions;
using Quivi.Hangfire.Hangfire;
using Quivi.Hangfire.Printing;
using Quivi.Hangfire.Settings;
using Quivi.Infrastructure.Abstractions.Services.Printing;
using Quivi.Printer.MassTransit.Extensions;

namespace Quivi.Hangfire
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.RegisterAll(builder.Configuration);
            builder.Services.AddHangfireServer();

            builder.Services.RegisterSingleton((p) => builder.Configuration.GetSection("PrinterConnector").Get<PrinterConnectorSettings>()!);
            builder.Services.ConfigurePrinterConnector<PrinterConnectorSettings>();
            builder.Services.AddScoped<IPrintingStatusUpdater, PrintingStatusUpdater>();

            var app = builder.Build();
            app.UseMiddleware<AuthenticateMiddleware>();
            app.UseHttpsRedirection();

            var authenticationfilters = new[]
            {
                new BasicAuthenticationFilter
                {
                    User = "admin",
                    Pass = "supersecret"
                },
            };
            app.UseHangfireDashboard("", new DashboardOptions
            {
                Authorization = authenticationfilters,
                AsyncAuthorization = authenticationfilters,
                DashboardTitle = "Quivi's Hangfire Dashboard",
            });
            app.Run();
        }
    }
}