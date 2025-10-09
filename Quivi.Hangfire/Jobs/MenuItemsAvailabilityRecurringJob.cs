using Hangfire;
using Quivi.Application.Queries.WeeklyAvailabilities;
using Quivi.Hangfire.Hangfire;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.MenuItems;

namespace Quivi.Hangfire.Jobs
{
    public class MenuItemsAvailabilityRecurringJob : IRecurringJob
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IEventService eventService;
        private readonly IDateTimeProvider dateTimeProvider;

        public string Schedule => Cron.Minutely();

        public MenuItemsAvailabilityRecurringJob(IQueryProcessor queryProcessor, IEventService eventService, IDateTimeProvider dateTimeProvider)
        {
            this.queryProcessor = queryProcessor;
            this.eventService = eventService;
            this.dateTimeProvider = dateTimeProvider;
        }

        public async Task Run()
        {
            var now = dateTimeProvider.GetUtcNow();
            var date = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

            var changingWeeklyAvailabilities = await queryProcessor.Execute(new GetWeeklyAvailabilitiesAsyncQuery
            {
                ChangesOnDate = [date],
                IncludeAvailabilityGroupAssociatedMenuItems = true,
                IncludeAvailabilityGroupAssociatedChannelProfiles = true,

                PageIndex = 0,
                PageSize = null,
            });

            var affectedMenuItems = changingWeeklyAvailabilities.Select(s => s.AvailabilityGroup!)
                                                                .SelectMany(e => e.AssociatedChannelProfiles!, (e, profile) => new
                                                                {
                                                                    AvailabilityGroup = e,
                                                                    ChannelProfileId = profile.ChannelProfileId,
                                                                    MerchantId = e.MerchantId,
                                                                })
                                                                .SelectMany(x => x.AvailabilityGroup.AssociatedMenuItems!, (x, item) => (item.MenuItemId, x.MerchantId, x.ChannelProfileId));

            foreach (var e in affectedMenuItems.Distinct())
                await eventService.Publish(new OnMenuItemAvailabilityChangedEvent
                {
                    MerchantId = e.MerchantId,
                    ChannelProfileId = e.ChannelProfileId,
                    Id = e.MenuItemId,
                });
        }
    }
}