using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.PrinterNotificationTargets;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Backoffice;

namespace Quivi.SignalR.EventHandlers.PrinterMessageTargets
{
    public class OnPrinterMessageTargetOperationEventHandler : IEventHandler<OnPrinterMessageTargetOperationEvent>
    {
        private readonly IHubContext<BackofficeHub, IBackofficeClient> backofficeHub;
        private readonly IIdConverter idConverter;

        public OnPrinterMessageTargetOperationEventHandler(IHubContext<BackofficeHub, IBackofficeClient> backofficeHub,
                                                            IIdConverter idConverter)
        {
            this.backofficeHub = backofficeHub;
            this.idConverter = idConverter;
        }

        public Task Process(OnPrinterMessageTargetOperationEvent message)
        {
            Dtos.OnPrinterMessageOperation evt = new Dtos.OnPrinterMessageOperation
            {
                Operation = message.Operation,
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                PrinterId = idConverter.ToPublicId(message.PrinterNotificationsContactId),
                MessageId = idConverter.ToPublicId(message.PrinterNotificationMessageId),
            };

            return backofficeHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnPrinterMessageOperation(evt);
            });
        }
    }
}
