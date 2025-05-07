using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Quivi.Infrastructure.Jobs.Hangfire.Context;

namespace Quivi.Infrastructure.Jobs.Hangfire.Filters
{
    /// <summary>
    /// Creates a value and contextualize it for other hangfire filters
    /// </summary>
    public class ContextualizeFilterAttribute : JobFilterAttribute, IServerFilter
    {
        private readonly string _contextualizerMethodName;

        private class JobContext : IJobContext, IJobContextualizer
        {
            public int? MerchantId { get; set; }
            public int? PosIntegrationId { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextualizerMethodName">
        /// The name of the method to run to obtain the values on which the lock should be based. The method should be non-static.
        /// The method first parameter should be an IJobContextualizer with the rest of the parameters being the same parameters of the method who has this annotation.
        /// </param>
        public ContextualizeFilterAttribute(string contextualizerMethodName)
        {
            this._contextualizerMethodName = contextualizerMethodName;
        }

        private void GetValue(PerformingContext filterContext, JobContext jobContext)
        {
            var job = filterContext.BackgroundJob.Job;
            if (string.IsNullOrEmpty(_contextualizerMethodName) == true)
                return;

            var methodInfo = job.Type.GetMethod(_contextualizerMethodName, job.Method.GetParameters().Select(p => p.ParameterType).Prepend(typeof(IJobContextualizer)).ToArray());
            using var scope = JobActivator.Current.BeginScope(filterContext);
            var jobInstance = scope.Resolve(job.Type);

            var objResult = methodInfo?.Invoke(jobInstance, job.Args.Prepend(jobContext).ToArray());
            if (objResult is Task t)
            {
                dynamic task = objResult;
                task.GetAwaiter().GetResult();
            }
        }

        public void OnPerforming(PerformingContext filterContext)
        {
            JobContext jobContext;
            if (filterContext.Items.TryGetValue("IJobContext", out object? contextObj) == false)
            {
                jobContext = new JobContext();
                filterContext.Items["IJobContext"] = jobContext;
            }
            else
                jobContext = (JobContext)contextObj;

            GetValue(filterContext, jobContext);
        }

        public void OnPerformed(PerformedContext filterContext)
        {
        }
    }
}
