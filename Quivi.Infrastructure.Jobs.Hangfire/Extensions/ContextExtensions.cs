using Hangfire.Server;
using Quivi.Infrastructure.Jobs.Hangfire.Context;

namespace Quivi.Infrastructure.Jobs.Hangfire.Extensions
{
    public static class ContextExtensions
    {
        public static IJobContext? GetContext(this PerformingContext filterContext)
        {
            if (filterContext.Items.TryGetValue("IJobContext", out object? contextObj) == false)
                return null;
            return (IJobContext?)contextObj;
        }

        public static IJobContext? GetContext(this PerformedContext filterContext)
        {
            if (filterContext.Items.TryGetValue("IJobContext", out object? contextObj) == false)
                return null;
            return (IJobContext?)contextObj;
        }
    }
}
