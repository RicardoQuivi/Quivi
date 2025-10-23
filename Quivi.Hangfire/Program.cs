using Hangfire;
using Quivi.Application.Extensions;
using Quivi.Hangfire.Extensions;
using Quivi.Hangfire.Hangfire;
using Quivi.Hangfire.Printing;
using Quivi.Hangfire.Settings;
using Quivi.Infrastructure.Abstractions.Services.Printing;
using Quivi.Infrastructure.Extensions;
using Quivi.Printer.MassTransit.Extensions;

namespace Quivi.Hangfire
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.RegisterAll(builder.Configuration);
            builder.Services.RegisterRecurringJobs();
            builder.Services.RegisterManagementPages();
            builder.Services.AddHangfireServer();
            builder.Services.AddControllers();

            builder.Services.RegisterSingleton((p) => builder.Configuration.GetSection("PrinterConnector").Get<PrinterConnectorSettings>()!);
            builder.Services.ConfigurePrinterConnector<PrinterConnectorSettings>();
            builder.Services.AddScoped<IPrintingStatusUpdater, PrintingStatusUpdater>();

            var app = builder.Build();
            app.UseMiddleware<AuthenticateMiddleware>();
            if (app.Environment.IsDevelopment() == false)
                app.UseHttpsRedirection();

            app.UseRouting();
            app.MapControllers();

            var authenticationfilters = new[]
            {
                new BasicAuthenticationFilter
                {
                    User = "admin",
                    Pass = "supersecret"
                },
            };
            app.MapHangfireDashboard("", new DashboardOptions
            {
                Authorization = authenticationfilters,
                AsyncAuthorization = authenticationfilters,
                DashboardTitle = "Quivi's Hangfire Dashboard",
            });


            await app.Services.AddRecurringJobs();
            app.Run();
        }
    }
}