using Quivi.Application.Extensions.Pos;
using Quivi.Application.Queries.MerchantInvoiceDocuments;
using Quivi.Application.Queries.PosCharges;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Pos.Invoicing;
using Quivi.Infrastructure.Abstractions.Pos.Invoicing.Models;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Application.Commands.Pos.Invoicing
{
    public class CreateInvoiceCancellationFromChargeAsyncCommand : ICommand<Task>
    {
        public required IInvoiceGateway InvoiceGateway { get; init; }
        public int PosChargeId { get; init; }
        public decimal RefundAmount { get; init; }
        public required string InvoicePrefix { get; init; }
        public bool IncludeTipInInvoice { get; init; }
        public required string Reason { get; init; }
    }

    public class CreateInvoiceCancellationFromChargeAsyncCommandHandler : ICommandHandler<CreateInvoiceCancellationFromChargeAsyncCommand, Task>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IDateTimeProvider dateTimeProvider;

        public CreateInvoiceCancellationFromChargeAsyncCommandHandler(IQueryProcessor queryProcessor, ICommandProcessor commandProcessor, IDateTimeProvider dateTimeProvider)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.dateTimeProvider = dateTimeProvider;
        }

        public async Task Handle(CreateInvoiceCancellationFromChargeAsyncCommand command)
        {
            var posCharge = await GetPosCharge(command.PosChargeId);

            if (posCharge.Charge!.Status != ChargeStatus.Completed)
                throw new Exception($"Trying to process a credit note for an incompleted charge with ChargeId {posCharge.Id}");

            Lazy<Task<InvoiceCancellation>> receipt = new Lazy<Task<InvoiceCancellation>>(() => CreateInvoiceCancellation(command, posCharge));

            await commandProcessor.Execute(new GetOrCreateMerchantInvoiceDocumentIdAsyncCommand
            {
                MerchantId = posCharge.MerchantId,
                PosChargeId = posCharge.Id,
                DocumentType = InvoiceDocumentType.InvoiceCancellation,
                CreatePdfDocument = async () =>
                {
                    var r = await receipt.Value;
                    return new CreateDocumentResponse
                    {
                        DocumentId = $"{r.DocumentId!}-voided",
                        FileData = await command.InvoiceGateway.GetInvoiceReceiptFile(r.DocumentId!, DocumentFileFormat.A4),
                    };
                },
                CreateEscPosDocument = async () =>
                {
                    var r = await receipt.Value;
                    return new CreateDocumentResponse
                    {
                        DocumentId = $"{r.DocumentId!}-voided",
                        FileData = await command.InvoiceGateway.GetInvoiceReceiptFile(r.DocumentId!, DocumentFileFormat.EscPOS),
                    };
                },
            });
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
        private async Task<InvoiceCancellation> CreateInvoiceCancellation(CreateInvoiceCancellationFromChargeAsyncCommand command, PosCharge posCharge)
        {
            var documentQuery = await queryProcessor.Execute(new GetMerchantInvoiceDocumentsAsyncQuery
            {
                MerchantIds = [posCharge!.MerchantId],
                PosChargeIds = [posCharge.Id],
                Types = [InvoiceDocumentType.OrderInvoice],
                PageSize = 1,
            });
            var relatedDocId = documentQuery.SingleOrDefault()?.DocumentId;

            if (string.IsNullOrWhiteSpace(relatedDocId))
                throw new Exception($"Cannot process Invoice Cancellation of Charge Id {posCharge.ChargeId} because no Document ID was found!");

            var invoiceReceipt = await command.InvoiceGateway.GetInvoiceReceipt(relatedDocId);
            if (invoiceReceipt == null)
                throw new Exception($"Cannot process Invoice Cancellation of Charge Id {posCharge.ChargeId} because no invoice was found!");

            var documentTotal = invoiceReceipt.Items.Sum(p => PriceHelper.CalculatePriceAfterDiscount(p.Price * p.Quantity, p.DiscountPercentage));
            var percentageRefund = command.RefundAmount / Math.Round(documentTotal, 2, MidpointRounding.ToEven);
            if (percentageRefund > 1)
                throw new Exception("Attempting to refund more than 100% total");

            return await command.InvoiceGateway.CreateInvoiceCancellation(new InvoiceCancellation
            {
                RelatedDocumentId = relatedDocId,
                CreatedDateUtc = dateTimeProvider.GetUtcNow(),
                SerieCode = command.InvoiceGateway.BuildCompleteSerieCode("QV", command.InvoicePrefix),
                PaymentMethodCode = posCharge.Charge!.ChargeMethod == ChargeMethod.Custom ? posCharge.Charge.MerchantCustomCharge!.CustomChargeMethod!.Name : "Quivi",
                Customer = new Customer(CustomerType.Personal)
                {
                    Code = posCharge.VatNumber,
                    VatNumber = posCharge.VatNumber,
                    Email = posCharge.Email,
                },
                PricesType = Infrastructure.Abstractions.Pos.Invoicing.Models.PriceType.IncludedTaxes,
                Items = invoiceReceipt.Items.Select(item => new InvoiceItem(item.Type)
                {
                    Reference = item.Reference,
                    CorrelationId = item.CorrelationId,
                    Name = item.Name,
                    Price = item.Price,
                    TaxPercentage = item.TaxPercentage,
                    Quantity = item.Quantity * percentageRefund,
                    DiscountPercentage = item.DiscountPercentage,
                }),
                Reason = command.Reason,
            });
        }
    }
}