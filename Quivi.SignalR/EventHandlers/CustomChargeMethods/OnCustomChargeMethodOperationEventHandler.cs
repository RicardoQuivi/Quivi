using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.CustomChargeMethods;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Backoffice;
using Quivi.SignalR.Hubs.Pos;

namespace Quivi.SignalR.EventHandlers.CustomChargeMethods
{
    public class OnCustomChargeMethodOperationEventHandler : IEventHandler<OnCustomChargeMethodOperationEvent>
    {
        private readonly IHubContext<BackofficeHub, IBackofficeClient> backofficeHub;
        private readonly IHubContext<PosHub, IPosClient> posHub;
        private readonly IIdConverter idConverter;

        public OnCustomChargeMethodOperationEventHandler(IHubContext<BackofficeHub, IBackofficeClient> backofficeHub,
                                                    IHubContext<PosHub, IPosClient> posHub,
                                                    IIdConverter idConverter)
        {
            this.backofficeHub = backofficeHub;
            this.posHub = posHub;
            this.idConverter = idConverter;
        }

        public async Task Process(OnCustomChargeMethodOperationEvent message)
        {
            Dtos.OnCustomChargeMethodOperation evt = new Dtos.OnCustomChargeMethodOperation
            {
                Operation = message.Operation,
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                Id = idConverter.ToPublicId(message.Id),
            };

            await backofficeHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnCustomChargeMethodOperation(evt);
            });

            await posHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnCustomChargeMethodOperation(evt);
            });
        }
    }
}