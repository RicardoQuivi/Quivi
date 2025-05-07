using System.Diagnostics;
using System.Reflection;

namespace Quivi.Application.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IEnumerable<Type> GetSafeAssembliesTypes(this AppDomain appDomain, Func<Assembly, bool>? assemblyFilter = null)
        {
            IEnumerable<Assembly> assemblies = appDomain.GetAssemblies();

            if (assemblyFilter != null)
                assemblies = assemblies.Where(assemblyFilter);

            return assemblies.SelectMany(GetTypes);
        }

        [DebuggerStepThrough]
        private static IEnumerable<Type> GetTypes(Assembly a)
        {
            try
            {
                return a.GetTypes();
            }
            catch (ReflectionTypeLoadException)
            {
                try
                {
                    return a.ExportedTypes;
                }
                catch (Exception)
                {
                    return Enumerable.Empty<Type>();
                }
            }
        }
    }
}
