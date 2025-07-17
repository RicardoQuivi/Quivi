using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.PosCharges;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Guests;

namespace Quivi.SignalR.EventHandlers.PosCharges
{
    public class OnPosChargeSyncedEventHandler : IEventHandler<OnPosChargeOperationEvent>
    {
        private readonly IHubContext<GuestsHub, IGuestClient> guestsHub;
        private readonly IIdConverter idConverter;

        public OnPosChargeSyncedEventHandler(IHubContext<GuestsHub, IGuestClient> guestsHub,
                                                IIdConverter idConverter)
        {
            this.guestsHub = guestsHub;
            this.idConverter = idConverter;
        }

        public async Task Process(OnPosChargeOperationEvent message)
        {
            Dtos.OnPosChargeOperation evt = new Dtos.OnPosChargeOperation
            {
                Operation = message.Operation,
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                ChannelId = idConverter.ToPublicId(message.ChannelId),
                Id = idConverter.ToPublicId(message.Id),
            };

            await guestsHub.WithChannelId(evt.ChannelId, async g =>
            {
                await g.Client.OnPosChargeOperation(evt);
            });
        }
    }
}