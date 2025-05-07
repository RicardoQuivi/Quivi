using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.PreparationGroups;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Pos;

namespace Quivi.SignalR.EventHandlers.PreparationGroups
{
    public class OnPreparationGroupOperationEventHandler : IEventHandler<OnPreparationGroupOperationEvent>
    {
        private readonly IHubContext<PosHub, IPosClient> posHub;
        private readonly IIdConverter idConverter;

        public OnPreparationGroupOperationEventHandler(IHubContext<PosHub, IPosClient> posHub,
                                                        IIdConverter idConverter)
        {
            this.posHub = posHub;
            this.idConverter = idConverter;
        }

        public async Task Process(OnPreparationGroupOperationEvent message)
        {
            Dtos.OnPreparationGroupOperation evt = new Dtos.OnPreparationGroupOperation
            {
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                Id = idConverter.ToPublicId(message.Id),
            };

            await posHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnPreparationGroupOperation(evt);
            });
        }
    }
}