using Quivi.Application.Queries.AvailabilityGroupChannelProfileAssociations;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.AvailabilityGroupMenuItemAssociations;
using Quivi.Infrastructure.Abstractions.Events.Data.MenuItems;

namespace Quivi.Hangfire.EventHandlers.AvailabilityMenuItemAssociations
{
    public class OnAvailabilityMenuItemAssociationOperationEventHandler : IEventHandler<OnAvailabilityMenuItemAssociationOperationEvent>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IEventService eventService;

        public OnAvailabilityMenuItemAssociationOperationEventHandler(IQueryProcessor queryProcessor, IEventService eventService)
        {
            this.queryProcessor = queryProcessor;
            this.eventService = eventService;
        }

        public async Task Process(OnAvailabilityMenuItemAssociationOperationEvent message)
        {
            await GenerateItemAvailabilities(message);
        }

        public async Task GenerateItemAvailabilities(OnAvailabilityMenuItemAssociationOperationEvent message)
        {
            var associatedChannelProfiles = await queryProcessor.Execute(new GetAvailabilityGroupChannelProfileAssociationsAsyncQuery
            {
                MerchantIds = [message.MerchantId],
                AvailabilityGroupIds = [message.AvailabilityGroupId],
                PageSize = null,
            });

            foreach (var association in associatedChannelProfiles)
                await eventService.Publish(new OnMenuItemAvailabilityChangedEvent
                {
                    MerchantId = message.MerchantId,
                    Id = message.MenuItemId,
                    ChannelProfileId = association.ChannelProfileId,
                });
        }
    }
}