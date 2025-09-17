using Microsoft.AspNetCore.SignalR;
using Quivi.Application.Queries.PosCharges;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.PosChargeSyncAttempts;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Backoffice;
using Quivi.SignalR.Hubs.Guests;
using Quivi.SignalR.Hubs.Pos;

namespace Quivi.SignalR.EventHandlers.PosCharges
{
    public class OnPosChargeSyncAttemptOperationEventHandler : IEventHandler<OnPosChargeSyncAttemptEvent>
    {
        private readonly IHubContext<BackofficeHub, IBackofficeClient> backofficeHub;
        private readonly IHubContext<PosHub, IPosClient> posHub;
        private readonly IHubContext<GuestsHub, IGuestClient> guestsHub;
        private readonly IIdConverter idConverter;
        private readonly IQueryProcessor queryProcessor;

        public OnPosChargeSyncAttemptOperationEventHandler(IHubContext<BackofficeHub, IBackofficeClient> backofficeHub,
                                                            IHubContext<PosHub, IPosClient> posHub,
                                                            IIdConverter idConverter,
                                                            IQueryProcessor queryProcessor,
                                                            IHubContext<GuestsHub, IGuestClient> guestsHub)
        {
            this.backofficeHub = backofficeHub;
            this.posHub = posHub;
            this.idConverter = idConverter;
            this.queryProcessor = queryProcessor;
            this.guestsHub = guestsHub;
        }

        public async Task Process(OnPosChargeSyncAttemptEvent message)
        {
            Dtos.OnPosChargeSyncAttemptOperation evt = new Dtos.OnPosChargeSyncAttemptOperation
            {
                Operation = message.Operation,
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                Id = idConverter.ToPublicId(message.Id),
                PosChargeId = idConverter.ToPublicId(message.PosChargeId),
            };

            await backofficeHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnPosChargeSyncAttemptOperation(evt);
            });

            await posHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnPosChargeSyncAttemptOperation(evt);
            });

            var posChargesQuery = await queryProcessor.Execute(new GetPosChargesAsyncQuery
            {
                Ids = [message.PosChargeId],
                PageSize = 1,
            });
            var posCharge = posChargesQuery.SingleOrDefault();

            if (posCharge == null)
                return;

            await guestsHub.WithChannelId(idConverter.ToPublicId(posCharge.ChannelId), async g =>
            {
                await g.Client.OnPosChargeSyncAttemptOperation(evt);
            });
            await GenerateSessionEvent(posCharge, message);
        }

        private async Task GenerateSessionEvent(PosCharge? posCharge, OnPosChargeSyncAttemptEvent message)
        {
            if (posCharge?.SessionId == null)
                return;

            Dtos.OnSessionUpdated evt = new Dtos.OnSessionUpdated
            {
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                ChannelId = idConverter.ToPublicId(posCharge.ChannelId),
                Id = idConverter.ToPublicId(posCharge.SessionId.Value),
            };

            await posHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnSessionUpdated(evt);
            });

            await guestsHub.WithChannelId(evt.ChannelId, async g =>
            {
                await g.Client.OnSessionUpdated(evt);
            });
        }
    }
}
