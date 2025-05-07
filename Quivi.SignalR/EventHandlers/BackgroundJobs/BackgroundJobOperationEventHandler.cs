using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.BackgroundJobs;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Pos;

namespace Quivi.SignalR.EventHandlers.BackgroundJobs
{
    public class BackgroundJobOperationEventHandler : IEventHandler<OnBackgroundJobOperationEvent>
    {
        private readonly IHubContext<PosHub, IPosClient> posHub;
        private readonly IIdConverter idConverter;

        public BackgroundJobOperationEventHandler(IHubContext<PosHub, IPosClient> posHub,
                                                    IIdConverter idConverter)
        {
            this.posHub = posHub;
            this.idConverter = idConverter;
        }

        public async Task Process(OnBackgroundJobOperationEvent message)
        {
            Dtos.OnBackgroundJobUpdated evt = new Dtos.OnBackgroundJobUpdated
            {
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                Id = message.Id,
            };

            await posHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnBackgroundJobUpdated(evt);
            });
        }
    }
}
