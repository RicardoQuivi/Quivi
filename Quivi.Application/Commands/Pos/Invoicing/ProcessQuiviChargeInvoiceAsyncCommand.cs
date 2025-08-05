using Quivi.Application.Extensions.Pos;
using Quivi.Application.Pos;
using Quivi.Application.Queries.PosCharges;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Pos.Invoicing.Models;
using Quivi.Infrastructure.Abstractions.Services;
using Quivi.Infrastructure.Extensions;
using gatewayInvoiving = Quivi.Infrastructure.Abstractions.Pos.Invoicing;
using gatewayModels = Quivi.Infrastructure.Abstractions.Pos.Invoicing.Models;

namespace Quivi.Application.Commands.Pos.Invoicing
{
    public class ProcessQuiviChargeInvoiceAsyncCommand : ICommand<Task>
    {
        public required gatewayInvoiving.IInvoiceGateway InvoiceGateway { get; init; }
        public required int PosChargeId { get; init; }
        public required decimal PaymentAmount { get; init; }
        public required IEnumerable<AQuiviSyncStrategy.InvoiceItem> InvoiceItems { get; init; }
        public required bool IncludeTip { get; init; }
        public required string InvoicePrefix { get; init; }
    }

    public class ProcessQuiviChargeInvoiceAsyncCommandHandler : ICommandHandler<ProcessQuiviChargeInvoiceAsyncCommand, Task>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IIdConverter idConverter;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly ILogger logger;

        public ProcessQuiviChargeInvoiceAsyncCommandHandler(IQueryProcessor queryProcessor,
                                                            ICommandProcessor commandProcessor,
                                                            IIdConverter idConverter,
                                                            IDateTimeProvider dateTimeProvider,
                                                            ILogger logger)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.idConverter = idConverter;

            this.dateTimeProvider = dateTimeProvider;
            this.logger = logger;
        }

        public async Task Handle(ProcessQuiviChargeInvoiceAsyncCommand command)
        {
            var posCharge = await GetPosCharge(command.PosChargeId);

            Lazy<Task<InvoiceReceipt>> receipt = new Lazy<Task<InvoiceReceipt>>(() => CreateInvoice(command, posCharge));
            await commandProcessor.Execute(new GetOrCreateInvoiceDocumentIdAsyncCommand
            {
                MerchantId = posCharge.MerchantId,
                PosChargeId = command.PosChargeId,
                DocumentType = InvoiceDocumentType.OrderInvoice,
                CreatePdfDocument = async () =>
                {
                    var r = await receipt.Value;
                    return new CreateDocumentResponse
                    {
                        DocumentId = r.DocumentId!,
                        FileData = await command.InvoiceGateway.GetInvoiceReceiptFile(r.DocumentId!, DocumentFileFormat.A4),
                    };
                },
                CreateEscPosDocument = async () =>
                {
                    var r = await receipt.Value;
                    return new CreateDocumentResponse
                    {
                        DocumentId = r.DocumentId!,
                        FileData = await command.InvoiceGateway.GetInvoiceReceiptFile(r.DocumentId!, DocumentFileFormat.EscPOS),
                    };
                },
            });

            // If invoice already exists
            if (receipt.IsValueCreated == false)
                return;

            var computedReceipt = await receipt.Value;
            decimal receiptPaymentAmount = computedReceipt.Items.Sum(p => PriceHelper.CalculatePriceAfterDiscount(p.Price * p.Quantity, p.DiscountPercentage));
            decimal errorMargin = 0.01M;
            if (Math.Abs(command.PaymentAmount - receiptPaymentAmount) >= errorMargin)
                logger.LogException(new Exception($"{GetType().Name}: This should never happen. If it does, it means the amount the receipt marks as paid ({receiptPaymentAmount}) is not the same as the total of the items ({command.PaymentAmount}). PosCharge: {command.PosChargeId}, document: {computedReceipt.DocumentId}"));
        }

        private async Task<PosCharge> GetPosCharge(int id)
        {
            var posChargeQuery = await queryProcessor.Execute(new GetPosChargesAsyncQuery
            {
                Ids = [id],
                IncludePosChargeInvoiceItems = true,
                IncludeMerchantCustomCharge = true,
                IncludeMerchantCustomChargeCustomChargeMethod = true,
                PageSize = 1,
            });
            return posChargeQuery.Single();
        }

        private async Task<InvoiceReceipt> CreateInvoice(ProcessQuiviChargeInvoiceAsyncCommand command, PosCharge posCharge)
        {
            var groupedItem = command.InvoiceItems.GroupBy(i => new
            {
                i.MenuItemId,
                i.Name,
                i.UnitPrice,
                i.VatRate,
                i.DiscountPercentage,
            }).Select(g => new AQuiviSyncStrategy.InvoiceItem
            {
                MenuItemId = g.Key.MenuItemId,
                Name = g.Key.Name,
                UnitPrice = g.Key.UnitPrice,
                VatRate = g.Key.VatRate,
                DiscountPercentage = g.Key.DiscountPercentage,
                Quantity = g.Sum(x => x.Quantity),
            })
                                                                .ToList();

            var invoiceItems = groupedItem.Select(g => new InvoiceItem(g.Type)
            {
                CorrelationId = idConverter.ToPublicId(g.MenuItemId),
                Name = g.Name,
                Price = g.UnitPrice,
                TaxPercentage = g.VatRate,
                Quantity = g.Quantity,
                DiscountPercentage = g.DiscountPercentage,
            }).ToList();

            if (command.IncludeTip && posCharge.Tip > 0.0M)
            {
                invoiceItems.Add(new InvoiceItem(InvoiceItemType.Services)
                {
                    CorrelationId = "Tip",
                    Name = "Gratificação",
                    Price = posCharge.Tip,
                    Quantity = 1,
                    TaxPercentage = 0,
                });
            }

            var charge = posCharge.Charge!;
            return await command.InvoiceGateway.CreateInvoiceReceipt(new InvoiceReceipt
            {
                CreatedDateUtc = dateTimeProvider.GetUtcNow(),
                PaymentMethodCode = charge.ChargeMethod == ChargeMethod.Custom ? charge.MerchantCustomCharge!.CustomChargeMethod!.Name : "Quivi",
                SerieCode = command.InvoiceGateway.BuildCompleteSerieCode("QV", command.InvoicePrefix),
                Customer = new Customer(CustomerType.Personal)
                {
                    Code = posCharge.VatNumber,
                    VatNumber = posCharge.VatNumber,
                    Email = posCharge.Email,
                },
                Notes = posCharge.Observations,
                PricesType = gatewayModels.PriceType.IncludedTaxes,
                Items = invoiceItems,
            });
        }
    }
}