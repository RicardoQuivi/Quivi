using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.Employees;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Backoffice;
using Quivi.SignalR.Hubs.Pos;

namespace Quivi.SignalR.EventHandlers.Employees
{
    public class OnEmployeeOperationEventHandler : IEventHandler<OnEmployeeOperationEvent>
    {
        private readonly IHubContext<BackofficeHub, IBackofficeClient> backofficeHub;
        private readonly IHubContext<PosHub, IPosClient> posHub;
        private readonly IIdConverter idConverter;

        public OnEmployeeOperationEventHandler(IHubContext<BackofficeHub, IBackofficeClient> backofficeHub,
                                                IHubContext<PosHub, IPosClient> posHub,
                                                IIdConverter idConverter)
        {
            this.backofficeHub = backofficeHub;
            this.posHub = posHub;
            this.idConverter = idConverter;
        }

        public async Task Process(OnEmployeeOperationEvent message)
        {
            Dtos.OnEmployeeOperation evt = new Dtos.OnEmployeeOperation
            {
                Operation = message.Operation,
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                Id = idConverter.ToPublicId(message.Id),
            };

            await backofficeHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnEmployeeOperation(evt);
            });
            await posHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnEmployeeOperation(evt);
            });
        }
    }
}
