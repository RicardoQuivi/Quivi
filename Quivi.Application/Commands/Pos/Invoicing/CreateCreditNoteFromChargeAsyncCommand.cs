using Quivi.Application.Extensions.Pos;
using Quivi.Application.Queries.Charges;
using Quivi.Application.Queries.MerchantInvoiceDocuments;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Pos.Invoicing;
using Quivi.Infrastructure.Abstractions.Pos.Invoicing.Models;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Application.Commands.Pos.Invoicing
{
    public class CreateCreditNoteFromChargeAsyncCommand : ICommand<Task>
    {
        public required IInvoiceGateway InvoiceGateway { get; init; }
        public int PosChargeId { get; init; }
        public decimal RefundAmount { get; init; }
        public required string InvoicePrefix { get; init; }
        public bool IncludeTipInInvoice { get; init; }
    }

    public class CreateCreditNoteFromChargeAsyncCommandHandler : ICommandHandler<CreateCreditNoteFromChargeAsyncCommand, Task>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IDateTimeProvider dateTimeProvider;

        public CreateCreditNoteFromChargeAsyncCommandHandler(IQueryProcessor queryProcessor, ICommandProcessor commandProcessor, IDateTimeProvider dateTimeProvider)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.dateTimeProvider = dateTimeProvider;
        }

        public async Task Handle(CreateCreditNoteFromChargeAsyncCommand command)
        {
            var chargeQuery = await queryProcessor.Execute(new GetChargesAsyncQuery
            {
                Ids = [command.PosChargeId],
                IncludePosCharge = true,
                IncludeMerchantAcquirerConfiguration = true,
            });

            var charge = chargeQuery.Single();
            if (charge.Status != ChargeStatus.Completed)
                throw new Exception($"Trying to process a credit note for an incompleted charge with ChargeId {charge.Id}");

            Lazy<Task<CreditNote>> receipt = new Lazy<Task<CreditNote>>(() => CreateCreditNote(command, charge));
            await commandProcessor.Execute(new GetOrCreateInvoiceDocumentIdAsyncCommand
            {
                MerchantId = charge.PosCharge!.MerchantId,
                PosChargeId = charge.Id,
                DocumentType = InvoiceDocumentType.CreditNote,
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
        }

        private async Task<CreditNote> CreateCreditNote(CreateCreditNoteFromChargeAsyncCommand command, Charge charge)
        {
            var documentQuery = await queryProcessor.Execute(new GetMerchantInvoiceDocumentsAsyncQuery
            {
                MerchantIds = [charge.PosCharge!.MerchantId],
                PosChargeIds = [charge.Id],
                Types = [InvoiceDocumentType.OrderInvoice],
                PageSize = 1,
            });
            var relatedDocId = documentQuery.SingleOrDefault()?.DocumentId;

            if (string.IsNullOrWhiteSpace(relatedDocId))
                throw new Exception($"Cannot process Credit Note of Deposit Id {charge.Id} because no Document ID was found for this Deposit Id!");

            var invoiceReceipt = await command.InvoiceGateway.GetInvoiceReceipt(relatedDocId);
            if (invoiceReceipt == null)
                throw new Exception($"Cannot process Credit Note of Deposit Id {charge.Id} because no invoice was found for this Deposit Id!");

            var documentTotal = invoiceReceipt.Items.Sum(p => PriceHelper.CalculatePriceAfterDiscount(p.Price * p.Quantity, p.DiscountPercentage));
            if (command.IncludeTipInInvoice)
                documentTotal += charge.PosCharge.Tip;

            var percentageRefund = command.RefundAmount / Math.Round(documentTotal, 2, MidpointRounding.ToEven);
            if (percentageRefund > 1)
                throw new Exception("Attempting to refund more than 100% total");

            return await command.InvoiceGateway.CreateCreditNote(new CreditNote
            {
                RelatedDocumentId = relatedDocId,
                CreatedDateUtc = dateTimeProvider.GetUtcNow(),
                SerieCode = command.InvoiceGateway.BuildCompleteSerieCode("QVCN", command.InvoicePrefix),
                PaymentMethodCode = charge.ChargeMethod == ChargeMethod.Custom ? charge.MerchantCustomCharge!.CustomChargeMethod!.Name : "Quivi",
                Customer = new Customer(CustomerType.Personal)
                {
                    Code = charge.PosCharge.VatNumber,
                    VatNumber = charge.PosCharge.VatNumber,
                    Email = charge.PosCharge.Email,
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
            });
        }
    }
}

