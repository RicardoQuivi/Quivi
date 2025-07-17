using Quivi.Application.Queries.Orders;
using Quivi.Application.Queries.PrinterNotificationsContacts;
using Quivi.Domain.Entities.Notifications;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.PrinterNotificationMessages;
using Quivi.Infrastructure.Abstractions.Events.Data.PrinterNotificationTargets;
using Quivi.Infrastructure.Abstractions.Pos.EscPos;
using Quivi.Infrastructure.Abstractions.Repositories;

namespace Quivi.Application.Commands.Orders
{
    public class PrintOrderPendingApprovalAsyncCommand : ICommand<Task>
    {
        public int OrderId { get; init; }
    }

    public class PrintOrderPendingApprovalAsyncCommandHandler : ICommandHandler<PrintOrderPendingApprovalAsyncCommand, Task>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IIdConverter idConverter;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IPrinterNotificationMessagesRepository repository;
        private readonly IEscPosPrinterService printerService;
        private readonly IEventService eventService;

        public PrintOrderPendingApprovalAsyncCommandHandler(IQueryProcessor queryProcessor,
                                                                IIdConverter idConverter,
                                                                IDateTimeProvider dateTimeProvider,
                                                                IEventService eventService,
                                                                IPrinterNotificationMessagesRepository repository,
                                                                IEscPosPrinterService printerService)
        {
            this.queryProcessor = queryProcessor;
            this.idConverter = idConverter;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
            this.repository = repository;
            this.printerService = printerService;
        }

        public async Task Handle(PrintOrderPendingApprovalAsyncCommand command)
        {
            var ordersQuery = await queryProcessor.Execute(new GetOrdersAsyncQuery
            {
                Ids = [command.OrderId],
                States = [OrderState.Requested],
                IncludeMerchant = true,
                IncludeChannelProfile = true,
                IncludeOrderSequence = true,
                PageIndex = 0,
                PageSize = 1,
            });

            var order = ordersQuery.SingleOrDefault();
            if (order == null)
                return;

            await GenerateMessages(order);
        }

        private async Task GenerateMessages(Order order)
        {
            var printers = await queryProcessor.Execute(new GetPrinterNotificationsContactsAsyncQuery
            {
                MerchantIds = [order.MerchantId],
                MessageTypes = [NotificationMessageType.NewOrder],
                IsDeleted = false,
                IncludeNotificationsContact = true,

                PageIndex = 0,
                PageSize = 1,
            });
            if (printers.Any() == false)
                return;

            var document = printerService.Get(new NewPendingOrderParameters
            {
                Timestamp = dateTimeProvider.GetNow(order.Merchant!.TimeZone),
                Title = "Pedido mobile pendente",
                OrderPlaceholder = order.OrderSequence?.SequenceNumber.ToString() ?? idConverter.ToPublicId(order.Id),
                ChannelPlaceholder = $"{order.Channel!.ChannelProfile!.Name} {order.Channel.Identifier}",
            });

            var now = dateTimeProvider.GetUtcNow();
            var entity = new PrinterNotificationMessage
            {
                MessageType = NotificationMessageType.NewOrder,
                ContentType = PrinterMessageContentType.EscPos,
                Content = document,
                MerchantId = order.MerchantId,
                PrinterMessageTargets = printers.Select(p => new PrinterMessageTarget
                {
                    CreatedDate = now,
                    ModifiedDate = now,
                    RequestedAt = null,
                    FinishedAt = null,
                    Status = AuditStatus.Pending,
                    PrinterNotificationsContactId = p.Id,
                }).ToList(),
                CreatedDate = now,
                ModifiedDate = now,
            };

            repository.Add(entity);
            await repository.SaveChangesAsync();

            await eventService.Publish(new OnPrinterNotificationMessageOperationEvent
            {
                Id = entity.Id,
                MerchantId = order.MerchantId,
                Operation = EntityOperation.Create,
            });

            foreach (var target in entity.PrinterMessageTargets!)
                await eventService.Publish(new OnPrinterMessageTargetOperationEvent
                {
                    PrinterNotificationMessageId = entity.Id,
                    PrinterNotificationsContactId = target.PrinterNotificationsContactId,
                    MerchantId = order.MerchantId,
                    Operation = EntityOperation.Create,
                });
        }
    }
}