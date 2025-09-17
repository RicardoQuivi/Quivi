using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.Sessions;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Guests;
using Quivi.SignalR.Hubs.Pos;

namespace Quivi.SignalR.EventHandlers.Sessions
{
    public class OnSessionOperationEventHandler : IEventHandler<OnSessionOperationEvent>
    {
        private readonly IHubContext<PosHub, IPosClient> posHub;
        private readonly IHubContext<GuestsHub, IGuestClient> guestsHub;
        private readonly IIdConverter idConverter;

        public OnSessionOperationEventHandler(IHubContext<PosHub, IPosClient> posHub,
                                            IHubContext<GuestsHub, IGuestClient> guestsHub,
                                            IIdConverter idConverter)
        {
            this.posHub = posHub;
            this.guestsHub = guestsHub;
            this.idConverter = idConverter;
        }

        public async Task Process(OnSessionOperationEvent message)
        {
            Dtos.OnSessionUpdated evt = new Dtos.OnSessionUpdated
            {
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                ChannelId = idConverter.ToPublicId(message.ChannelId),
                Id = idConverter.ToPublicId(message.Id),
            };

            await posHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnSessionUpdated(evt);
            });

            await guestsHub.WithChannelId(evt.ChannelId, async g =>
            {
                await g.Client.OnSessionUpdated(evt);
            });
        }
    }
}