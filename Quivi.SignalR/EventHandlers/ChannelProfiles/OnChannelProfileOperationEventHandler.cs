using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.ChannelProfiles;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Backoffice;
using Quivi.SignalR.Hubs.Pos;

namespace Quivi.SignalR.EventHandlers.ChannelProfiles
{
    public class OnChannelProfileOperationEventHandler : IEventHandler<OnChannelProfileOperationEvent>
    {
        private readonly IHubContext<BackofficeHub, IBackofficeClient> backofficeHub;
        private readonly IHubContext<PosHub, IPosClient> posHub;
        private readonly IIdConverter idConverter;

        public OnChannelProfileOperationEventHandler(IHubContext<BackofficeHub, IBackofficeClient> backofficeHub,
                                                            IHubContext<PosHub, IPosClient> posHub,
                                                            IIdConverter idConverter)
        {
            this.backofficeHub = backofficeHub;
            this.idConverter = idConverter;
            this.posHub = posHub;
        }

        public async Task Process(OnChannelProfileOperationEvent message)
        {
            Dtos.OnChannelProfileOperation evt = new Dtos.OnChannelProfileOperation
            {
                Operation = message.Operation,
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                Id = idConverter.ToPublicId(message.Id),
            };

            await backofficeHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnChannelProfileOperation(evt);
            });

            await posHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnChannelProfileOperation(evt);
            });
        }
    }
}
