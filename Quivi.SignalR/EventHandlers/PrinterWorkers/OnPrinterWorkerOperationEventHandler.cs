using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.PrinterWorkers;
using Quivi.SignalR.Extensions;
using Quivi.SignalR.Hubs.Backoffice;

namespace Quivi.SignalR.EventHandlers.PrinterWorkers
{
    public class OnPrinterWorkerOperationEventHandler : IEventHandler<OnPrinterWorkerOperationEvent>
    {
        private readonly IHubContext<BackofficeHub, IBackofficeClient> backofficeHub;
        private readonly IIdConverter idConverter;

        public OnPrinterWorkerOperationEventHandler(IHubContext<BackofficeHub, IBackofficeClient> backofficeHub,
                                                        IIdConverter idConverter)
        {
            this.backofficeHub = backofficeHub;
            this.idConverter = idConverter;
        }

        public Task Process(OnPrinterWorkerOperationEvent message)
        {
            Dtos.OnPrinterWorkerOperation evt = new Dtos.OnPrinterWorkerOperation
            {
                Operation = message.Operation,
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                Id = idConverter.ToPublicId(message.Id),
            };

            return backofficeHub.WithMerchantId(evt.MerchantId, async g =>
            {
                await g.Client.OnPrinterWorkerOperation(evt);
            });
        }
    }
}