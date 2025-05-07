using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.Merchants;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Backoffice;

namespace Quivi.SignalR.EventHandlers.Merchants
{
    public class OnMerchantAssociatedOperationEventHandler : IEventHandler<OnMerchantAssociatedOperationEvent>
    {
        private readonly IHubContext<BackofficeHub, IBackofficeClient> backofficeHub;
        private readonly IIdConverter idConverter;

        public OnMerchantAssociatedOperationEventHandler(IHubContext<BackofficeHub, IBackofficeClient> backofficeHub,
                                                            IIdConverter idConverter)
        {
            this.backofficeHub = backofficeHub;
            this.idConverter = idConverter;
        }

        public Task Process(OnMerchantAssociatedOperationEvent message)
        {
            switch (message.Operation)
            {
                case EntityOperation.Create: return OnCreate(message);
            }
            return Task.CompletedTask;
        }

        private async Task OnCreate(OnMerchantAssociatedOperationEvent message)
        {
            Dtos.OnMerchantAssociatedOperation evt = new Dtos.OnMerchantAssociatedOperation
            {
                Operation = message.Operation,
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                UserId = idConverter.ToPublicId(message.UserId),
            };

            await backofficeHub.WithUserId(evt.UserId, async g =>
            {
                await g.Client.OnMerchantAssociated(evt);
            });
        }
    }
}
