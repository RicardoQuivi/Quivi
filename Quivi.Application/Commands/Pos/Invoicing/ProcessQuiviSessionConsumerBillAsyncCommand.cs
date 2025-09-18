using Quivi.Application.Extensions;
using Quivi.Application.Extensions.Pos;
using Quivi.Application.Queries.Orders;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Pos.Invoicing.Models;
using System.Text;
using gatewayInvoicing = Quivi.Infrastructure.Abstractions.Pos.Invoicing;

namespace Quivi.Application.Commands.Pos.Invoicing
{
    public class ProcessQuiviSessionConsumerBillAsyncCommand : ICommand<Task<byte[]?>>
    {
        public required gatewayInvoicing.IInvoiceGateway InvoiceGateway { get; init; }
        public required int SessionId { get; set; }
        public required string InvoicePrefix { get; set; }
    }

    public class ProcessQuiviSessionConsumerBillAsyncCommandHandler : ICommandHandler<ProcessQuiviSessionConsumerBillAsyncCommand, Task<byte[]?>>
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

        public async Task<byte[]?> Handle(ProcessQuiviSessionConsumerBillAsyncCommand command)
        {
            var ordersQuery = await queryProcessor.Execute(new GetOrdersAsyncQuery
            {
                SessionIds = [command.SessionId],
                IncludeOrderMenuItemsPosChargeInvoiceItems = true,
                PageSize = null,
            });
            var sessionItems = ordersQuery.GetValidOrderMenuItems().AsConvertedSessionItems();
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

            var totalPending = unpaidItems.Sum(r => r.SessionItem.GetUnitPrice() * r.SessionItem.Quantity);
            if (totalPending == 0)
                return null;

            var bill = await command.InvoiceGateway.CreateConsumerBillReceipt(new ConsumerBill
            {
                CreatedDateUtc = dateTimeProvider.GetUtcNow(),
                SerieCode = command.InvoiceGateway.BuildCompleteSerieCode("QV", command.InvoicePrefix),
                PricesType = gatewayInvoicing.Models.PriceType.IncludedTaxes,
                Items = ToInvoiceItems(unpaidItems.Select(o => o.SessionItem)),
            });

            var base64ByteContent = await command.InvoiceGateway.GetConsumerBillFile(bill.DocumentId!, DocumentFileFormat.EscPOS);
            var base64Content = Encoding.UTF8.GetString(base64ByteContent);
            var decodedBytes = Convert.FromBase64String(base64Content);
            return decodedBytes;
        }

        private IEnumerable<InvoiceItem> ToInvoiceItems(IEnumerable<SessionItem<OrderMenuItem, OrderMenuItem>> sessionItems)
        {
            foreach (var sessionItem in sessionItems)
            {
                var firstItem = sessionItem.Source.First();
                yield return new InvoiceItem(InvoiceItemType.ProcessedProducts)
                {
                    Reference = idConverter.ToPublicId(sessionItem.MenuItemId),
                    CorrelationId = idConverter.ToPublicId(sessionItem.MenuItemId),
                    Name = firstItem.Name,
                    Price = sessionItem.Price,
                    TaxPercentage = firstItem.VatRate,
                    Quantity = sessionItem.Quantity,
                    DiscountPercentage = sessionItem.Discount,
                };

                foreach (var extra in sessionItem.Extras)
                {
                    var firstExtra = extra.Source.First();
                    yield return new InvoiceItem(InvoiceItemType.ProcessedProducts)
                    {
                        Reference = idConverter.ToPublicId(extra.MenuItemId),
                        CorrelationId = idConverter.ToPublicId(extra.MenuItemId),
                        Name = firstExtra.Name,
                        Price = extra.Price,
                        TaxPercentage = firstExtra.VatRate,
                        Quantity = extra.Quantity * sessionItem.Quantity,
                    };
                }
            }
        }
    }
}