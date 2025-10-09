using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.AvailabilityGroupMenuItemAssociations;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Backoffice;

namespace Quivi.SignalR.EventHandlers.AvailabilityMenuItemAssociations
{
    public class OnAvailabilityMenuItemAssociationOperationEventHandler : IEventHandler<OnAvailabilityMenuItemAssociationOperationEvent>
    {
        private readonly IHubContext<BackofficeHub, IBackofficeClient> backofficeHub;
        private readonly IIdConverter idConverter;

        public OnAvailabilityMenuItemAssociationOperationEventHandler(IHubContext<BackofficeHub, IBackofficeClient> backofficeHub, IIdConverter idConverter)
        {
            this.backofficeHub = backofficeHub;
            this.idConverter = idConverter;
        }

        public async Task Process(OnAvailabilityMenuItemAssociationOperationEvent message)
        {
            Dtos.OnAvailabilityMenuItemAssociationOperation evt = new Dtos.OnAvailabilityMenuItemAssociationOperation
            {
                Operation = message.Operation,
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                AvailabilityId = idConverter.ToPublicId(message.AvailabilityGroupId),
                MenuItemId = idConverter.ToPublicId(message.MenuItemId),
            };

            await backofficeHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnAvailabilityMenuItemAssociationOperation(evt);
            });
        }
    }
}