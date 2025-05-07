using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.PosCharges;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Backoffice;
using Quivi.SignalR.Hubs.Pos;

namespace Quivi.SignalR.EventHandlers.PosCharges
{
    public class OnPosChargeOperationEventHandler : IEventHandler<OnPosChargeOperationEvent>
    {
        private readonly IHubContext<BackofficeHub, IBackofficeClient> backofficeHub;
        private readonly IHubContext<PosHub, IPosClient> posHub;
        private readonly IIdConverter idConverter;

        public OnPosChargeOperationEventHandler(IHubContext<BackofficeHub, IBackofficeClient> backofficeHub,
                                                    IHubContext<PosHub, IPosClient> posHub,
                                                    IIdConverter idConverter)
        {
            this.backofficeHub = backofficeHub;
            this.posHub = posHub;
            this.idConverter = idConverter;
        }

        public async Task Process(OnPosChargeOperationEvent message)
        {
            Dtos.OnPosChargeOperation evt = new Dtos.OnPosChargeOperation
            {
                Operation = message.Operation,
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                Id = idConverter.ToPublicId(message.Id),
            };

            await backofficeHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnPosChargeOperation(evt);
            });

            await posHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnPosChargeOperation(evt);
            });
        }
    }
}
