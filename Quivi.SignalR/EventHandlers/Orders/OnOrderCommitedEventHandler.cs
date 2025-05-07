using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.Orders;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Pos;

namespace Quivi.SignalR.EventHandlers.Orders
{
    public class OnOrderCommitedEventHandler : IEventHandler<OnOrderCommitedEvent>
    {
        private readonly IHubContext<PosHub, IPosClient> posHub;
        private readonly IIdConverter idConverter;

        public OnOrderCommitedEventHandler(IHubContext<PosHub, IPosClient> posHub,
                                            IIdConverter idConverter)
        {
            this.posHub = posHub;
            this.idConverter = idConverter;
        }

        public async Task Process(OnOrderCommitedEvent message)
        {
            Dtos.OnSessionUpdated evt = new Dtos.OnSessionUpdated
            {
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                ChannelId = idConverter.ToPublicId(message.ChannelId),
                Id = idConverter.ToPublicId(message.SessionId),
            };

            await posHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnSessionUpdated(evt);
            });
        }
    }
}