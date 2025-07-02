using Quivi.Application.Commands.PrinterMessageTargets;
using Quivi.Application.Queries.PrinterMessageTargets;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.PrinterNotificationMessages;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Services.Printing;

namespace Quivi.Hangfire.EventHandlers.PrinterNotificationMessages
{
    public class OnPrinterNotificationMessageOperationEventHandler : IEventHandler<OnPrinterNotificationMessageOperationEvent>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IPrinterMessageConnector printerMessageDispatcher;
        private readonly IBackgroundJobHandler backgroundJobHandler;
        private readonly IDateTimeProvider dateTimeProvider;

        public OnPrinterNotificationMessageOperationEventHandler(IQueryProcessor queryProcessor,
                                                                    ICommandProcessor commandProcessor,
                                                                    IPrinterMessageConnector printerMessageDispatcher,
                                                                    IBackgroundJobHandler backgroundJobHandler,
                                                                    IDateTimeProvider dateTimeProvider)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.printerMessageDispatcher = printerMessageDispatcher;
            this.backgroundJobHandler = backgroundJobHandler;
            this.dateTimeProvider = dateTimeProvider;
        }

        public async Task Process(OnPrinterNotificationMessageOperationEvent evt)
        {
            if (evt.Operation != Infrastructure.Abstractions.Events.Data.EntityOperation.Create)
                return;

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

            backgroundJobHandler.Schedule(() => ProcessTimeout(evt), TimeSpan.FromSeconds(60));
        }

        public async Task ProcessTimeout(OnPrinterNotificationMessageOperationEvent evt)
        {
            var now = dateTimeProvider.GetUtcNow();
            await commandProcessor.Execute(new UpdatePrinterMessageTargetsAsyncCommand
            {
                Criteria = new GetPrinterMessageTargetsCriteria
                {
                    PrinterNotificationMessageIds = [evt.Id],
                    PageSize = 1,
                    PageIndex = 0,
                },
                UpdateAction = (e) =>
                {
                    if (e.FinishedAt.HasValue)
                        return Task.CompletedTask;

                    e.FinishedAt = now;
                    e.Status = Domain.Entities.Notifications.AuditStatus.TimedOut;
                    return Task.CompletedTask;
                }
            });
        }
    }
}