using Quivi.Application.Extensions;
using Quivi.Hangfire.Hangfire;
using System.Reflection;

namespace Quivi.Hangfire.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterRecurringJobs(this IServiceCollection serviceCollection)
        {
            Type interfaceType = typeof(IRecurringJob);
            var classesTypes = Assembly.GetEntryAssembly()!.GetTypes()
                                        .Where(t => t.IsClass)
                                        .Where(t => !t.IsAbstract)
                                        .Where(t => !t.IsGenericType)
                                        .Where(t => t.FilterTypeInterfaces(interfaceType).Any());

            foreach (var implementation in classesTypes)
                serviceCollection.RegisterScoped(implementation);

            serviceCollection.RegisterScoped<IEnumerable<IRecurringJob>>(s =>
            {
                List<IRecurringJob> result = new();
                foreach (var implementation in classesTypes)
                    result.Add((IRecurringJob)s.GetService(implementation)!);
                return result;
            });
        }

        private static IEnumerable<Type> FilterTypeInterfaces(this Type type, Type interfaceType)
        {
            IEnumerable<Type> interfaces = type.GetInterfaces();

            interfaces = interfaceType.IsGenericType
                ? interfaces.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType)
                : interfaces.Where(i => i == interfaceType);

            return interfaces;
        }
    }
}