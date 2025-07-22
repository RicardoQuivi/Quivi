using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.Reviews;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Backoffice;
using Quivi.SignalR.Hubs.Guests;

namespace Quivi.SignalR.EventHandlers.Reviews
{
    public class OnReviewOperationEventHandler : IEventHandler<OnReviewOperationEvent>
    {
        private readonly IHubContext<BackofficeHub, IBackofficeClient> backofficeHub;
        private readonly IHubContext<GuestsHub, IGuestClient> guestsHub;
        private readonly IIdConverter idConverter;

        public OnReviewOperationEventHandler(IHubContext<BackofficeHub, IBackofficeClient> backofficeHub,
                                                IHubContext<GuestsHub, IGuestClient> guestsHub,
                                                IIdConverter idConverter)
        {
            this.backofficeHub = backofficeHub;
            this.guestsHub = guestsHub;
            this.idConverter = idConverter;
        }

        public async Task Process(OnReviewOperationEvent message)
        {
            Dtos.OnReviewOperation evt = new Dtos.OnReviewOperation
            {
                Operation = message.Operation,
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                ChannelId = idConverter.ToPublicId(message.ChannelId),
                Id = idConverter.ToPublicId(message.Id),
            };

            await backofficeHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnReviewOperation(evt);
            });
        }
    }
}