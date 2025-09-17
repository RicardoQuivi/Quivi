using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.MerchantInvoiceDocuments;
using Quivi.SignalR.Dtos.Guests;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Backoffice;
using Quivi.SignalR.Hubs.Guests;

namespace Quivi.SignalR.EventHandlers.MerchantInvoiceDocuments
{
    public class OnMerchantInvoiceDocumentOperationEventHandler : IEventHandler<OnMerchantInvoiceDocumentOperationEvent>
    {
        private readonly IHubContext<BackofficeHub, IBackofficeClient> backofficeHub;
        private readonly IHubContext<GuestsHub, IGuestClient> guestsHub;
        private readonly IIdConverter idConverter;

        public OnMerchantInvoiceDocumentOperationEventHandler(IHubContext<BackofficeHub, IBackofficeClient> backofficeHub,
                                                IHubContext<GuestsHub, IGuestClient> guestsHub,
                                                IIdConverter idConverter)
        {
            this.backofficeHub = backofficeHub;
            this.guestsHub = guestsHub;
            this.idConverter = idConverter;
        }

        public async Task Process(OnMerchantInvoiceDocumentOperationEvent message)
        {
            Dtos.OnMerchantDocumentOperation evt = new Dtos.OnMerchantDocumentOperation
            {
                Operation = message.Operation,
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                PosChargeId = message.PosChargeId.HasValue ? idConverter.ToPublicId(message.PosChargeId.Value) : null,
                Id = idConverter.ToPublicId(message.Id),
            };

            await backofficeHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnMerchantDocumentOperation(evt);
            });

            if (evt.PosChargeId == null)
                return;

            await guestsHub.WithTransactionId(evt.PosChargeId, async g =>
            {
                await g.Client.OnTransactionInvoiceOperation(new OnTransactionInvoiceOperation
                {
                    Id = evt.Id,
                    MerchantId = evt.MerchantId,
                    PosChargeId = evt.PosChargeId,
                });
            });
        }
    }
}