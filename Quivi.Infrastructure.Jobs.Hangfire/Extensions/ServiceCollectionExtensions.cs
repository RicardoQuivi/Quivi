using Hangfire;
using Hangfire.Dashboard.Management.v2;
using Hangfire.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Jobs.Hangfire.Activators;
using Quivi.Infrastructure.Jobs.Hangfire.Filters;
using System.Reflection;

namespace Quivi.Infrastructure.Jobs.Hangfire.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterHangfireJobHandler(this IServiceCollection serviceCollection, string connectionString)
        {
            serviceCollection.AddScoped<MerchantBackgroundJobPublisher>();
            serviceCollection.AddSingleton<IBackgroundJobHandler, HangfireJobHandler>();

            serviceCollection.AddHangfire((IServiceProvider serviceProvider, IGlobalConfiguration configuration) =>
            {
                var jobActivator = new ServiceProviderJobActivator(serviceProvider);
                configuration.UseActivator(jobActivator).ConfigureHangfire(connectionString);
                configuration.UseManagementPages(Assembly.GetEntryAssembly());

                //GlobalConfiguration.Configuration.UseActivator(jobActivator);
            });

            //GlobalConfiguration.Configuration.ConfigureHangfire(connectionString);
        }

        private static IGlobalConfiguration<SqlServerStorage> ConfigureHangfire(this IGlobalConfiguration configuration, string connectionString)
        {
            return configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                                .UseSimpleAssemblyNameTypeSerializer()
                                .UseFilter(new MerchantBackgroundJobEventFilter())
                                .UseRecommendedSerializerSettings(c =>
                                {
                                    c.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
                                    c.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                                    c.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;
                                })
                                .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                                {
                                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                                    QueuePollInterval = TimeSpan.Zero,
                                    UseRecommendedIsolationLevel = true,
                                    DisableGlobalLocks = true,
                                    JobExpirationCheckInterval = TimeSpan.FromMinutes(30),
                                });
        }
    }
}
