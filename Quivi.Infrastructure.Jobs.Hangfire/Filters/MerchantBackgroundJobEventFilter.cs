using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Quivi.Infrastructure.Jobs.Hangfire.Extensions;

namespace Quivi.Infrastructure.Jobs.Hangfire.Filters
{
    public class MerchantBackgroundJobEventFilter : JobFilterAttribute, IServerFilter
    {
        public MerchantBackgroundJobEventFilter()
        {
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            var merchantId = filterContext.GetContext()?.MerchantId;
            if (merchantId.HasValue == false)
                return;

            string jobId = filterContext.BackgroundJob.Id;
            int merchantIdValue = merchantId.Value;

            BackgroundJob.Enqueue<MerchantBackgroundJobPublisher>(p => p.PublishEvent(jobId, merchantIdValue));
        }

        public void OnPerforming(PerformingContext filterContext)
        {
        }
    }
}