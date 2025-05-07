using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.Channels;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Backoffice;
using Quivi.SignalR.Hubs.Pos;

namespace Quivi.SignalR.EventHandlers.Channels
{
    public class OnChannelOperationEventHandler : IEventHandler<OnChannelOperationEvent>
    {
        private readonly IHubContext<BackofficeHub, IBackofficeClient> backofficeHub;
        private readonly IHubContext<PosHub, IPosClient> posHub;
        private readonly IIdConverter idConverter;

        public OnChannelOperationEventHandler(IHubContext<BackofficeHub, IBackofficeClient> backofficeHub,
                                                IHubContext<PosHub, IPosClient> posHub,
                                                IIdConverter idConverter)
        {
            this.backofficeHub = backofficeHub;
            this.posHub = posHub;
            this.idConverter = idConverter;
        }

        public async Task Process(OnChannelOperationEvent message)
        {
            Dtos.OnChannelOperation evt = new Dtos.OnChannelOperation
            {
                Operation = message.Operation,
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                Id = idConverter.ToPublicId(message.Id),
            };

            await backofficeHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnChannelOperation(evt);
            });

            await posHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnChannelOperation(evt);
            });
        }
    }
}