using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.AvailabilityGroupChannelProfileAssociations;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Backoffice;

namespace Quivi.SignalR.EventHandlers.AvailabilityProfileAssociations
{
    public class OnAvailabilityChannelProfileAssociationOperationEventHandler : IEventHandler<OnAvailabilityChannelProfileAssociationOperationEvent>
    {
        private readonly IHubContext<BackofficeHub, IBackofficeClient> backofficeHub;
        private readonly IIdConverter idConverter;

        public OnAvailabilityChannelProfileAssociationOperationEventHandler(IHubContext<BackofficeHub, IBackofficeClient> backofficeHub, IIdConverter idConverter)
        {
            this.backofficeHub = backofficeHub;
            this.idConverter = idConverter;
        }

        public async Task Process(OnAvailabilityChannelProfileAssociationOperationEvent message)
        {
            Dtos.OnAvailabilityChannelProfileAssociationOperation evt = new Dtos.OnAvailabilityChannelProfileAssociationOperation
            {
                Operation = message.Operation,
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                AvailabilityId = idConverter.ToPublicId(message.AvailabilityGroupId),
                ChannelProfileId = idConverter.ToPublicId(message.ChannelProfileId),
            };

            await backofficeHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnAvailabilityChannelProfileAssociationOperation(evt);
            });
        }
    }
}