using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.BackgroundJobs;

namespace Quivi.Infrastructure.Jobs.Hangfire
{
    public class MerchantBackgroundJobPublisher
    {
        private readonly IEventService eventService;

        public MerchantBackgroundJobPublisher(IEventService eventService)
        {
            this.eventService = eventService;
        }

        public Task PublishEvent(string jobId, int merchantId) => eventService.Publish(new OnBackgroundJobOperationEvent
        {
            Operation = EntityOperation.Update,
            Id = jobId,
            MerchantId = merchantId,
        });
    }
}