using Quivi.Application.Pos;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using gatewayInvoiving = Quivi.Infrastructure.Abstractions.Pos.Invoicing;
using gatewayModels = Quivi.Infrastructure.Abstractions.Pos.Invoicing.Models;
using Quivi.Infrastructure.Extensions;
using Quivi.Application.Queries.PosCharges;
using Quivi.Domain.Entities.Charges;
using Quivi.Application.Extensions.Pos;
using Quivi.Infrastructure.Abstractions.Services;

namespace Quivi.Application.Commands.Pos.Invoicing
{
    public class ProcessQuiviChargeInvoiceAsyncCommand : ICommand<Task>
    {
        public required gatewayInvoiving.IInvoiceGateway InvoiceGateway { get; init; }
        public required int PosChargeId { get; init; }
        public required decimal PaymentAmount { get; init; }
        public required IEnumerable<AQuiviSyncStrategy.InvoiceItem> InvoiceItems { get; init; }
        public required bool IncludeSurcharge { get; init; }
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
            gatewayModels.InvoiceReceipt? receipt = null;
            var posCharge = await GetPosCharge(command.PosChargeId);
            string documentId = await commandProcessor.Execute(new GetOrCreateInvoiceDocumentIdAsyncCommand
            {
                MerchantId = posCharge.MerchantId,
                PosChargeId = command.PosChargeId,
                DocumentType = InvoiceDocumentType.OrderInvoice,
                CreateNewDocumentIdAction = async () =>
                {
                    receipt = await CreateInvoice(command, posCharge);
                    return receipt.DocumentId!;
                },
            });

            // If invoice already exists
            if (receipt == null)
                return;

            decimal receiptPaymentAmount = receipt.Items.Sum(p => PriceHelper.CalculatePriceAfterDiscount(p.Price * p.Quantity, p.DiscountPercentage));
            decimal errorMargin = 0.01M;
            if (Math.Abs(command.PaymentAmount - receiptPaymentAmount) >= errorMargin)
                logger.LogException(new Exception($"{GetType().Name}: This should never happen. If it does, it means the amount the receipt marks as paid ({receiptPaymentAmount}) is not the same as the total of the items ({command.PaymentAmount}). PosCharge: {command.PosChargeId}, document: {documentId}"));
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

        private async Task<gatewayModels.InvoiceReceipt> CreateInvoice(ProcessQuiviChargeInvoiceAsyncCommand command, PosCharge posCharge)
        {
            var invoiceItems = command.InvoiceItems.Select(g => new gatewayModels.InvoiceItem(g.Type)
            {
                CorrelationId = idConverter.ToPublicId(g.MenuItemId),
                Name = g.Name,
                Price = g.UnitPrice,
                TaxPercentage = g.VatRate,
                Quantity = g.Quantity,
                DiscountPercentage = g.DiscountPercentage,
            }).ToList();

            if (command.IncludeSurcharge && posCharge.SurchargeFeeAmount > 0.0M)
            {
                invoiceItems.Add(new gatewayModels.InvoiceItem(gatewayModels.InvoiceItemType.Services)
                {
                    CorrelationId = "ConvenienceFee",
                    Name = "Taxa de Conveniência",
                    Price = posCharge.SurchargeFeeAmount,
                    Quantity = 1,
                    TaxPercentage = 23,
                    DiscountPercentage = 0,
                });
            }

            if (command.IncludeTip && posCharge.Tip > 0.0M)
            {
                invoiceItems.Add(new gatewayModels.InvoiceItem(gatewayModels.InvoiceItemType.Services)
                {
                    CorrelationId = "Tip",
                    Name = "Gratificação",
                    Price = posCharge.Tip,
                    Quantity = 1,
                    TaxPercentage = 0,
                });
            }

            var charge = posCharge.Charge!;
            return await command.InvoiceGateway.CreateInvoiceReceipt(new gatewayModels.InvoiceReceipt
            {
                CreatedDateUtc = dateTimeProvider.GetUtcNow(),
                PaymentMethodCode = charge.ChargeMethod == ChargeMethod.Custom ? charge.MerchantCustomCharge!.CustomChargeMethod!.Name : "Quivi",
                SerieCode = command.InvoiceGateway.BuildCompleteSerieCode("QV", command.InvoicePrefix),
                Customer = new gatewayModels.Customer(gatewayModels.CustomerType.Personal)
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