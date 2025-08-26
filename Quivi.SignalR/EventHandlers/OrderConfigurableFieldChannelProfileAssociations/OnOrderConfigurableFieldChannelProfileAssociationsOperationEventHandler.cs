using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.OrderConfigurableFieldChannelProfileAssociations;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Backoffice;
using Quivi.SignalR.Hubs.Guests;
using Quivi.SignalR.Hubs.Pos;

namespace Quivi.SignalR.EventHandlers.OrderConfigurableFieldChannelProfileAssociations
{
    public class OnOrderConfigurableFieldChannelProfileAssociationsOperationEventHandler : IEventHandler<OnOrderConfigurableFieldChannelProfileAssociationOperationEvent>
    {
        private readonly IHubContext<BackofficeHub, IBackofficeClient> backofficeHub;
        private readonly IHubContext<PosHub, IPosClient> posHub;
        private readonly IHubContext<GuestsHub, IGuestClient> guestsHub;
        private readonly IIdConverter idConverter;

        public OnOrderConfigurableFieldChannelProfileAssociationsOperationEventHandler(IHubContext<BackofficeHub, IBackofficeClient> backofficeHub,
                                                                                        IHubContext<PosHub, IPosClient> posHub,
                                                                                        IHubContext<GuestsHub, IGuestClient> guestsHub,
                                                                                        IIdConverter idConverter)
        {
            this.backofficeHub = backofficeHub;
            this.posHub = posHub;
            this.idConverter = idConverter;
            this.guestsHub = guestsHub;
        }

        public async Task Process(OnOrderConfigurableFieldChannelProfileAssociationOperationEvent message)
        {
            Dtos.OnConfigurableFieldAssociationOperation evt = new Dtos.OnConfigurableFieldAssociationOperation
            {
                Operation = message.Operation,
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                ChannelProfileId = idConverter.ToPublicId(message.ChannelProfileId),
                ConfigurableFieldId = idConverter.ToPublicId(message.OrderConfigurableFieldId),
            };

            await backofficeHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnConfigurableFieldAssociationOperation(evt);
            });

            await posHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnConfigurableFieldAssociationOperation(evt);
            });

            await guestsHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnConfigurableFieldAssociationOperation(evt);
            });
        }
    }
}
