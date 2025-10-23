using ComplyPay;
using ComplyPay.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Quivi.Infrastructure.Abstractions.Payouts;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Infrastructure.Payouts.ComplyPay.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterComplyPay<T>(this IServiceCollection serviceCollection) where T : IComplyPaySettings
        {
            serviceCollection.RegisterScoped<IComplyPaySettings>(p => p.GetService<T>()!);
            serviceCollection.RegisterScoped<IComplyPayApi>((p) =>
            {
                var settings = p.GetService<IComplyPaySettings>()!;
                return new ComplyPayApi(settings.Host);
            });
            serviceCollection.RegisterScoped<IComplyPayService>((p) =>
            {
                var settings = p.GetService<IComplyPaySettings>()!;
                var api = p.GetService<IComplyPayApi>()!;
                return new ComplyPayService(api, settings.Email, settings.Password);
            });
            serviceCollection.RegisterScoped<IPayoutProcessor, ComplyPayApiPayoutProcessor>();

            return serviceCollection;
        }
    }
}