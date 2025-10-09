using Quivi.Application.Queries.AvailabilityGroupMenuItemAssociations;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.AvailabilityGroupChannelProfileAssociations;
using Quivi.Infrastructure.Abstractions.Events.Data.MenuItems;

namespace Quivi.Hangfire.EventHandlers.AvailabilityProfileAssociations
{
    public class OnAvailabilityChannelProfileAssociationOperationEventHandler : IEventHandler<OnAvailabilityChannelProfileAssociationOperationEvent>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IEventService eventService;

        public OnAvailabilityChannelProfileAssociationOperationEventHandler(IQueryProcessor queryProcessor, IEventService eventService)
        {
            this.queryProcessor = queryProcessor;
            this.eventService = eventService;
        }

        public async Task Process(OnAvailabilityChannelProfileAssociationOperationEvent message)
        {
            await GenerateItemAvailabilities(message);
        }

        public async Task GenerateItemAvailabilities(OnAvailabilityChannelProfileAssociationOperationEvent message)
        {
            var associatedMenuItems = await queryProcessor.Execute(new GetAvailabilityGroupMenuItemAssociationsAsyncQuery
            {
                MerchantIds = [message.MerchantId],
                AvailabilityGroupIds = [message.AvailabilityGroupId],
                PageSize = null,
            });

            foreach (var association in associatedMenuItems)
                await eventService.Publish(new OnMenuItemAvailabilityChangedEvent
                {
                    MerchantId = message.MerchantId,
                    Id = association.MenuItemId,
                    ChannelProfileId = message.ChannelProfileId,
                });
        }
    }
}