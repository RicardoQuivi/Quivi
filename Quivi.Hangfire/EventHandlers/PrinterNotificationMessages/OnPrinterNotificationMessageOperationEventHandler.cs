using Quivi.Application.Queries.PrinterMessageTargets;
using Quivi.Domain.Entities.Notifications;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.PrinterNotificationMessages;
using Quivi.Infrastructure.Abstractions.Services.Printing;
using Quivi.Printer.Contracts;

namespace Quivi.Hangfire.EventHandlers.PrinterNotificationMessages
{
    public class OnPrinterNotificationMessageOperationEventHandler : IEventHandler<OnPrinterNotificationMessageOperationEvent>
    {
        private readonly IIdConverter idConverter;
        private readonly IQueryProcessor queryProcessor;
        private readonly IPrinterMessageConnector printerMessageDispatcher;

        public OnPrinterNotificationMessageOperationEventHandler(IIdConverter idConverter,
                                                                    IQueryProcessor queryProcessor,
                                                                    IPrinterMessageConnector printerMessageDispatcher)
        {
            this.idConverter = idConverter;
            this.queryProcessor = queryProcessor;
            this.printerMessageDispatcher = printerMessageDispatcher;
        }

        public async Task Process(OnPrinterNotificationMessageOperationEvent evt)
        {
            var messageTargetsQuery = await queryProcessor.Execute(new GetPrinterMessageTargetsAsyncQuery
            {
                PrinterNotificationMessageIds = [evt.Id],
                DeletedTargets = false,

                IncludePrinterNotificationMessage = true,
                IncludePrinterNotificationsContactPrinterWorker = true,
                PageIndex = 0,
                PageSize = null,
            });

            var messagesPerWorker = messageTargetsQuery.GroupBy(g => g.PrinterNotificationsContact!.PrinterWorker!)
                                                        .ToDictionary(g => g.Key, g => g.AsEnumerable());

            foreach (var entry in messagesPerWorker)
            {
                var worker = entry.Key;
                var messageTargets = entry.Value;
                var message = entry.Value.Select(e => e.PrinterNotificationMessage!).First();

                await printerMessageDispatcher.SendMessage(worker.Identifier, new Message
                {
                    PrinterNotificationMessageId = evt.Id,
                    MerchantId = evt.MerchantId,
                    Content = message.Content,
                    Targets = messageTargets.Select(t => new Target
                    {
                        PrinterNotificationsContactId = t.PrinterNotificationsContactId,
                        Address = t.PrinterNotificationsContact!.Address,
                    }).ToList(),
                    CreatedDate = new DateTimeOffset(message.CreatedDate, TimeSpan.Zero),
                });
            }
        }
    }
}
