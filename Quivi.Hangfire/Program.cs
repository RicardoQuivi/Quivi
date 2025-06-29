using Hangfire;
using Quivi.Application.Extensions;
using Quivi.Hangfire.Hangfire;
using Quivi.Hangfire.Printing;
using Quivi.Infrastructure.Abstractions.Services.Printing;

namespace Quivi.Hangfire
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.RegisterAll(builder.Configuration);
            builder.Services.AddHangfireServer();
            builder.Services.AddSingleton<IPrintingStatusUpdater, PrintingStatusUpdater>();

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