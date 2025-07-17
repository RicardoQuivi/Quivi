using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.MerchantAcquirerConfigurations;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Backoffice;
using Quivi.SignalR.Hubs.Guests;

namespace Quivi.SignalR.EventHandlers.MerchantAcquirerConfigurations
{
    public class OnMerchantAcquirerConfigurationEventHandler : IEventHandler<OnMerchantAcquirerConfigurationEvent>
    {
        private readonly IIdConverter idConverter;
        private readonly IHubContext<BackofficeHub, IBackofficeClient> backofficeHub;
        private readonly IHubContext<GuestsHub, IGuestClient> guestsHub;

        public OnMerchantAcquirerConfigurationEventHandler(IHubContext<BackofficeHub, IBackofficeClient> backofficeHub,
                                                           IHubContext<GuestsHub, IGuestClient> guestsHub,
                                                           IIdConverter idConverter)
        {
            this.backofficeHub = backofficeHub;
            this.guestsHub = guestsHub;
            this.idConverter = idConverter;
        }

        public async Task Process(OnMerchantAcquirerConfigurationEvent message)
        {
            Dtos.OnAcquirerConfigurationOperation evt = new Dtos.OnAcquirerConfigurationOperation
            {
                Operation = message.Operation,
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                Id = idConverter.ToPublicId(message.Id),
            };

            await backofficeHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnAcquirerConfigurationOperation(evt);
            });

            await guestsHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnAcquirerConfigurationOperation(evt);
            });
        }
    }
}