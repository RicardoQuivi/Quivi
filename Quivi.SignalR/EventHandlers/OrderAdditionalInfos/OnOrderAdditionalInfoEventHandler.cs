using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.OrderAdditionalFields;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Pos;

namespace Quivi.SignalR.EventHandlers.OrderAdditionalInfos
{
    public class OnOrderAdditionalInfoEventHandler : IEventHandler<OnOrderAdditionalInfoOperationEvent>
    {
        private readonly IHubContext<PosHub, IPosClient> posHub;
        private readonly IIdConverter idConverter;
        public OnOrderAdditionalInfoEventHandler(IHubContext<PosHub, IPosClient> posHub,
                                                    IIdConverter idConverter)
        {
            this.posHub = posHub;
            this.idConverter = idConverter;
        }

        public Task Process(OnOrderAdditionalInfoOperationEvent message)
        {
            Dtos.OnOrderAdditionalInfoOperation evt = new Dtos.OnOrderAdditionalInfoOperation
            {
                Operation = message.Operation,
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                OrderConfigurableFieldId = idConverter.ToPublicId(message.OrderConfigurableFieldId),
                OrderId = idConverter.ToPublicId(message.OrderId),
            };

            return posHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnOrderAdditionalInfoOperation(evt);
            });
        }
    }
}