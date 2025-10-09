using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.MenuItems;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Guests;

namespace Quivi.SignalR.EventHandlers.MenuItems
{
    public class OnMenuItemAvailabilityChangedEventHandler : IEventHandler<OnMenuItemAvailabilityChangedEvent>
    {
        private readonly IHubContext<GuestsHub, IGuestClient> guestsHub;
        private readonly IIdConverter idConverter;

        public OnMenuItemAvailabilityChangedEventHandler(IHubContext<GuestsHub, IGuestClient> guestsHub, IIdConverter idConverter)
        {
            this.guestsHub = guestsHub;
            this.idConverter = idConverter;
        }

        public async Task Process(OnMenuItemAvailabilityChangedEvent message)
        {
            Dtos.Guests.OnMenuItemAvailabilityChanged evt = new Dtos.Guests.OnMenuItemAvailabilityChanged
            {
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                ChannelProfileId = idConverter.ToPublicId(message.ChannelProfileId),
                Id = idConverter.ToPublicId(message.Id),
            };

            await guestsHub.WithChannelProfileId(evt.ChannelProfileId, async g =>
            {
                await g.Client.OnMenuItemAvailabilityChanged(evt);
            });
        }
    }
}