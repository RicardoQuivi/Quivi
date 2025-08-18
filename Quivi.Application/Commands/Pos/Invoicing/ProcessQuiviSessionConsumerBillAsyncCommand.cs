using Quivi.Application.Extensions.Pos;
using Quivi.Application.Queries.Orders;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Pos.Invoicing.Models;
using System.Text;
using gatewayInvoicing = Quivi.Infrastructure.Abstractions.Pos.Invoicing;

namespace Quivi.Application.Commands.Pos.Invoicing
{
    public class ProcessQuiviSessionConsumerBillAsyncCommand : ICommand<Task<string?>>
    {
        public required gatewayInvoicing.IInvoiceGateway InvoiceGateway { get; init; }
        public required int SessionId { get; set; }
        public required string InvoicePrefix { get; set; }
    }

    public class ProcessQuiviSessionConsumerBillAsyncCommandHandler : ICommandHandler<ProcessQuiviSessionConsumerBillAsyncCommand, Task<string?>>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IIdConverter idConverter;

        public ProcessQuiviSessionConsumerBillAsyncCommandHandler(IQueryProcessor queryProcessor,
                                                                    IDateTimeProvider dateTimeProvider,
                                                                    IIdConverter idConverter)
        {
            this.queryProcessor = queryProcessor;
            this.dateTimeProvider = dateTimeProvider;
            this.idConverter = idConverter;
        }

        public async Task<string?> Handle(ProcessQuiviSessionConsumerBillAsyncCommand command)
        {
            var ordersQuery = await queryProcessor.Execute(new GetOrdersAsyncQuery
            {
                SessionIds = [command.SessionId],
                IncludeOrderMenuItemsPosChargeInvoiceItems = true,
                PageSize = null,
            });
            var sessionItems = ordersQuery.SelectMany(o => o.OrderMenuItems!).AsConvertedSessionItems();
            var unpaidItems = sessionItems.Select(item =>
            {
                var paidQuantity = item.Source.SelectMany(s => s.PosChargeInvoiceItems!).Sum(s => s.Quantity);
                var unpaidQuantity = item.Quantity - paidQuantity;

                return new
                {
                    SessionItem = item,
                    UnpaidQuantity = unpaidQuantity,
                };
            }).Where(s => s.UnpaidQuantity > 0).ToList();

            if (unpaidItems.Sum(r => r.SessionItem.GetUnitPrice() * r.SessionItem.Quantity) == 0)
                return null;

            var bill = await command.InvoiceGateway.CreateConsumerBillReceipt(new ConsumerBill
            {
                CreatedDateUtc = dateTimeProvider.GetUtcNow(),
                SerieCode = command.InvoiceGateway.BuildCompleteSerieCode("QV", command.InvoicePrefix),
                PricesType = PriceType.IncludedTaxes,
                Items = unpaidItems.Select(o =>
                {
                    var firstItem = o.SessionItem.Source.First();
                    return new InvoiceItem(InvoiceItemType.ProcessedProducts)
                    {
                        Reference = idConverter.ToPublicId(o.SessionItem.MenuItemId),
                        CorrelationId = idConverter.ToPublicId(o.SessionItem.MenuItemId),
                        Name = firstItem.Name,
                        Price = o.SessionItem.Price,
                        TaxPercentage = firstItem.VatRate,
                        Quantity = o.SessionItem.Quantity,
                        DiscountPercentage = o.SessionItem.Discount,
                    };
                }),
            });

            var file = await command.InvoiceGateway.GetConsumerBillFile(bill.DocumentId!, DocumentFileFormat.EscPOS);
            var base64Content = Encoding.UTF8.GetString(file);
            return base64Content;
        }
    }
}