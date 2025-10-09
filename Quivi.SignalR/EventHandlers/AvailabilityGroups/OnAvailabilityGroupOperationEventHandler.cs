using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.AvailabilityGroups;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Backoffice;

namespace Quivi.SignalR.EventHandlers.AvailabilityGroups
{
    public class OnAvailabilityGroupOperationEventHandler : IEventHandler<OnAvailabilityGroupOperationEvent>
    {
        private readonly IHubContext<BackofficeHub, IBackofficeClient> backofficeHub;
        private readonly IIdConverter idConverter;

        public OnAvailabilityGroupOperationEventHandler(IHubContext<BackofficeHub, IBackofficeClient> backofficeHub, IIdConverter idConverter)
        {
            this.backofficeHub = backofficeHub;
            this.idConverter = idConverter;
        }

        public async Task Process(OnAvailabilityGroupOperationEvent message)
        {
            Dtos.OnAvailabilityOperation evt = new Dtos.OnAvailabilityOperation
            {
                Operation = message.Operation,
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                Id = idConverter.ToPublicId(message.Id),
            };

            await backofficeHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnAvailabilityOperation(evt);
            });
        }
    }
}