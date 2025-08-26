using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.OrderConfigurableFields;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Backoffice;
using Quivi.SignalR.Hubs.Guests;
using Quivi.SignalR.Hubs.Pos;

namespace Quivi.SignalR.EventHandlers.OrderConfigurableFields
{
    public class OnOrderConfigurableFieldOperationEventHandler : IEventHandler<OnOrderConfigurableFieldOperationEvent>
    {
        private readonly IHubContext<BackofficeHub, IBackofficeClient> backofficeHub;
        private readonly IHubContext<PosHub, IPosClient> posHub;
        private readonly IHubContext<GuestsHub, IGuestClient> guestsHub;
        private readonly IIdConverter idConverter;

        public OnOrderConfigurableFieldOperationEventHandler(IHubContext<BackofficeHub, IBackofficeClient> backofficeHub,
                                                                IHubContext<PosHub, IPosClient> posHub,
                                                                IHubContext<GuestsHub, IGuestClient> guestsHub,
                                                                IIdConverter idConverter)
        {
            this.backofficeHub = backofficeHub;
            this.posHub = posHub;
            this.idConverter = idConverter;
            this.guestsHub = guestsHub;
        }

        public async Task Process(OnOrderConfigurableFieldOperationEvent message)
        {
            Dtos.OnConfigurableFieldOperation evt = new Dtos.OnConfigurableFieldOperation
            {
                Operation = message.Operation,
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                Id = idConverter.ToPublicId(message.Id),
            };

            await backofficeHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnConfigurableFieldOperation(evt);
            });

            await posHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnConfigurableFieldOperation(evt);
            });

            await guestsHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnConfigurableFieldOperation(evt);
            });
        }
    }
}