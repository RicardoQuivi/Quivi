using Microsoft.Extensions.DependencyInjection;

namespace Quivi.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterSingleton<TService>(this IServiceCollection services) where TService : class
        {
            services.AddSingleton<TService>();
        }

        public static void RegisterSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, TService> factory) where TService : class
        {
            services.AddSingleton(factory);
        }

        public static void RegisterSingleton<TService, TImplementation>(this IServiceCollection services) where TService : class
                                                                                                            where TImplementation : class, TService
        {
            services.AddSingleton<TService, TImplementation>();
            services.AddSingleton<TImplementation>();
        }

        public static void RegisterScoped<TService>(this IServiceCollection services) where TService : class
        {
            services.AddScoped<TService>();
        }

        public static void RegisterScoped<TService>(this IServiceCollection services, Func<IServiceProvider, TService> factory) where TService : class
        {
            services.AddScoped(factory);
        }

        public static void RegisterScoped<TService, TImplementation>(this IServiceCollection services) where TService : class
                                                                                                            where TImplementation : class, TService
        {
            services.AddScoped<TService, TImplementation>();
            services.AddScoped<TImplementation>();
        }

        public static void RegisterScoped(this IServiceCollection services, Type serviceType)
        {
            services.AddScoped(serviceType);
        }

        public static void RegisterScoped(this IServiceCollection services, Type serviceType, Type implementationType)
        {
            services.AddScoped(serviceType, implementationType);
            services.AddScoped(implementationType);
        }

        public static void RegisterScoped(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> factory)
        {
            services.AddScoped(serviceType, factory);
        }
    }
}