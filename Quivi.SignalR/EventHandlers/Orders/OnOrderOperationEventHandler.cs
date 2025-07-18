﻿using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.Orders;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Guests;
using Quivi.SignalR.Hubs.Pos;

namespace Quivi.SignalR.EventHandlers.Orders
{
    public class OnOrderOperationEventHandler : IEventHandler<OnOrderOperationEvent>
    {
        private readonly IHubContext<PosHub, IPosClient> posHub;
        private readonly IHubContext<GuestsHub, IGuestClient> guestHub;
        private readonly IIdConverter idConverter;

        public OnOrderOperationEventHandler(IHubContext<PosHub, IPosClient> posHub,
                                            IIdConverter idConverter,
                                            IHubContext<GuestsHub, IGuestClient> guestHub)
        {
            this.posHub = posHub;
            this.idConverter = idConverter;
            this.guestHub = guestHub;
        }

        public async Task Process(OnOrderOperationEvent message)
        {
            Dtos.OnOrderOperation evt = new Dtos.OnOrderOperation
            {
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                ChannelId = idConverter.ToPublicId(message.ChannelId),
                Id = idConverter.ToPublicId(message.Id),
            };

            await posHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnOrderOperation(evt);
            });

            await guestHub.WithChannelId(evt.ChannelId, async g =>
            {
                await g.Client.OnOrderOperation(evt);
            });
        }
    }
}