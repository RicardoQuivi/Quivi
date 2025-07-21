using Quivi.Application.Commands.PrinterNotificationMessages;
using Quivi.Application.Queries.Orders;
using Quivi.Domain.Entities.Notifications;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Pos.EscPos;

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
        private readonly IEscPosPrinterService printerService;
        private readonly ICommandProcessor commandProcessor;

        public PrintOrderPendingApprovalAsyncCommandHandler(IQueryProcessor queryProcessor,
                                                                IIdConverter idConverter,
                                                                IDateTimeProvider dateTimeProvider,
                                                                IEscPosPrinterService printerService,
                                                                ICommandProcessor commandProcessor)
        {
            this.queryProcessor = queryProcessor;
            this.idConverter = idConverter;
            this.dateTimeProvider = dateTimeProvider;
            this.printerService = printerService;
            this.commandProcessor = commandProcessor;
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

            await commandProcessor.Execute(new CreatePrinterNotificationMessageAsyncCommand
            {
                MessageType = NotificationMessageType.NewOrder,
                Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetPrinterNotificationsContactsCriteria
                {
                    MerchantIds = [order.MerchantId],
                    MessageTypes = [NotificationMessageType.ConsumerBill],
                    IsDeleted = false,

                    PageIndex = 0,
                    PageSize = null,
                },
                GetContent = () => Task.FromResult<string?>(printerService.Get(new NewPendingOrderParameters
                {
                    Timestamp = dateTimeProvider.GetNow(order.Merchant!.TimeZone),
                    Title = "Pedido mobile pendente",
                    OrderPlaceholder = order.OrderSequence?.SequenceNumber.ToString() ?? idConverter.ToPublicId(order.Id),
                    ChannelPlaceholder = $"{order.Channel!.ChannelProfile!.Name} {order.Channel.Identifier}",
                }))
            });
        }
    }
}