using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.Locations;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Backoffice;
using Quivi.SignalR.Hubs.Pos;

namespace Quivi.SignalR.EventHandlers.Locations
{
    public class OnLocationOperationEventHandler : IEventHandler<OnLocationOperationEvent>
    {
        private readonly IHubContext<BackofficeHub, IBackofficeClient> backofficeHub;
        private readonly IHubContext<PosHub, IPosClient> posHub;
        private readonly IIdConverter idConverter;

        public OnLocationOperationEventHandler(IHubContext<BackofficeHub, IBackofficeClient> backofficeHub,
                                                    IHubContext<PosHub, IPosClient> posHub,
                                                    IIdConverter idConverter)
        {
            this.backofficeHub = backofficeHub;
            this.posHub = posHub;
            this.idConverter = idConverter;
        }

        public async Task Process(OnLocationOperationEvent message)
        {
            Dtos.OnLocationOperation evt = new Dtos.OnLocationOperation
            {
                Operation = message.Operation,
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                Id = idConverter.ToPublicId(message.Id),
            };

            await backofficeHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnLocationOperation(evt);
            });

            await posHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnLocationOperation(evt);
            });
        }
    }
}
