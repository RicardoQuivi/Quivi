using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.PrinterNotificationsContacts;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Backoffice;

namespace Quivi.SignalR.EventHandlers.OnPrinterNotificationsContacts
{
    public class OnPrinterNotificationsContactOperationEventHandler : IEventHandler<OnPrinterNotificationsContactOperationEvent>
    {
        private readonly IHubContext<BackofficeHub, IBackofficeClient> backofficeHub;
        private readonly IIdConverter idConverter;

        public OnPrinterNotificationsContactOperationEventHandler(IHubContext<BackofficeHub, IBackofficeClient> backofficeHub,
                                                        IIdConverter idConverter)
        {
            this.backofficeHub = backofficeHub;
            this.idConverter = idConverter;
        }

        public Task Process(OnPrinterNotificationsContactOperationEvent message)
        {
            Dtos.OnPrinterOperation evt = new Dtos.OnPrinterOperation
            {
                Operation = message.Operation,
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                Id = idConverter.ToPublicId(message.Id),
            };

            return backofficeHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnPrinterOperation(evt);
            });
        }
    }
}