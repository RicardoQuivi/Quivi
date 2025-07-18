using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.PosCharges;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Backoffice;
using Quivi.SignalR.Hubs.Guests;

namespace Quivi.SignalR.EventHandlers.PosCharges
{
    public class OnPosChargeSyncedEventHandler : IEventHandler<OnPosChargeOperationEvent>
    {
        private readonly IHubContext<BackofficeHub, IBackofficeClient> backofficeHub;
        private readonly IHubContext<GuestsHub, IGuestClient> guestsHub;
        private readonly IIdConverter idConverter;

        public OnPosChargeSyncedEventHandler(IHubContext<GuestsHub, IGuestClient> guestsHub,
                                                IIdConverter idConverter,
                                                IHubContext<BackofficeHub, IBackofficeClient> backofficeHub)
        {
            this.guestsHub = guestsHub;
            this.idConverter = idConverter;
            this.backofficeHub = backofficeHub;
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

            await backofficeHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnPosChargeOperation(evt);
            });

            await guestsHub.WithChannelId(evt.ChannelId, async g =>
            {
                await g.Client.OnPosChargeOperation(evt);
            });
        }
    }
}