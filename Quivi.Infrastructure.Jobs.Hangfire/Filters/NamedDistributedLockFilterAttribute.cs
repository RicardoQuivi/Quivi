using Hangfire.Common;
using Hangfire.Server;
using Quivi.Infrastructure.Jobs.Hangfire.Context;
using Quivi.Infrastructure.Jobs.Hangfire.Extensions;
using System.Diagnostics;

namespace Quivi.Infrastructure.Jobs.Hangfire.Filters
{
    /// <summary>
    /// Creates a named distributed lock related to the provided values.
    /// </summary>
    public class NamedDistributedLockFilterAttribute : JobFilterAttribute, IServerFilter
    {
        private readonly string _name;
        private readonly TimeSpan _timeout;
        private readonly Func<IJobContext, object?>? _additionalFilter;

        /// <summary>
        /// Creates a Named distributed lock with the provided name
        /// </summary>
        /// <param name="name">The name of this distributed lock.</param>
        /// <param name="additionalFilter">A function providing an additional filter from the IJobContext.</param>
        public NamedDistributedLockFilterAttribute(string name, Func<IJobContext, object?> additionalFilter)
        {
            _name = name;
            _timeout = TimeSpan.MaxValue;
            _additionalFilter = additionalFilter;
        }

        public NamedDistributedLockFilterAttribute(string name)
        {
            _name = name;
            _timeout = TimeSpan.MaxValue;
        }

        private string LockName => $"{this.GetType()}[{_name}]";

        private string GetResource(PerformingContext filterContext)
        {
            var job = filterContext.BackgroundJob.Job;
            string id = $"{typeof(NamedDistributedLockFilterAttribute).FullName}[{_name}]";
            if (_additionalFilter == null)
                return $"{id}";

            var jobContext = filterContext.GetContext();
            if(jobContext == null)
                return $"{id}";

            var objResult = _additionalFilter(jobContext);
            if (objResult == null)
                return $"{id}";

            return $"{id} {objResult}";
        }

        [DebuggerStepThrough]
        public void OnPerforming(PerformingContext filterContext)
        {
            string resource = GetResource(filterContext);
            var distributedLock = filterContext.Connection.AcquireDistributedLock(resource, _timeout);
            filterContext.Items[LockName] = distributedLock;
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            if (!filterContext.Items.ContainsKey(LockName))
                throw new InvalidOperationException("Can not release a distributed lock: it was not acquired.");

            var distributedLock = (IDisposable)filterContext.Items[LockName];
            distributedLock.Dispose();
        }
    }
}
