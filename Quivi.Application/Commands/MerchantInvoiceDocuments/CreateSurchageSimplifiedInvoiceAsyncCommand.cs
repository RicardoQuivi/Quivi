using Quivi.Application.Commands.Pos.Invoicing;
using Quivi.Application.Extensions.Pos;
using Quivi.Application.Queries.PosCharges;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Pos.Invoicing;
using Quivi.Infrastructure.Abstractions.Pos.Invoicing.Models;

namespace Quivi.Application.Commands.MerchantInvoiceDocuments
{
    public class CreateSurchageSimplifiedInvoiceAsyncCommand : ICommand<Task>
    {
        public int PosChargeId { get; init; }
    }

    public class CreateSurchageSimplifiedInvoiceAsyncCommandHandler : ICommandHandler<CreateSurchageSimplifiedInvoiceAsyncCommand, Task>
    {
        private readonly ICommandProcessor commandProcessor;
        private readonly IQueryProcessor queryProcessor;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IInvoiceGateway invoiceGateway;
        private readonly IIdConverter idConverter;

        public CreateSurchageSimplifiedInvoiceAsyncCommandHandler(ICommandProcessor commandProcessor,
                                                                    IQueryProcessor queryProcessor,
                                                                    IDateTimeProvider dateTimeProvider,
                                                                    IInvoiceGateway invoiceGateway,
                                                                    IIdConverter idConverter)
        {
            this.commandProcessor = commandProcessor;
            this.queryProcessor = queryProcessor;
            this.dateTimeProvider = dateTimeProvider;
            this.invoiceGateway = invoiceGateway;
            this.idConverter = idConverter;
        }

        public async Task Handle(CreateSurchageSimplifiedInvoiceAsyncCommand command)
        {
            var posCharge = await GetPosCharge(command.PosChargeId);
            await commandProcessor.Execute(new GetOrCreateMerchantInvoiceDocumentIdAsyncCommand
            {
                MerchantId = posCharge.MerchantId,
                PosChargeId = posCharge.Id,
                DocumentType = InvoiceDocumentType.SurchargeInvoice,
                CreatePdfDocument = async () =>
                {
                    var documentId = await CreateSimplifiedInvoice(posCharge);
                    return new CreateDocumentResponse
                    {
                        DocumentId = documentId,
                        FileData = await invoiceGateway.GetInvoiceReceiptFile(documentId, DocumentFileFormat.A4),
                    };
                },
            });
        }

        private async Task<PosCharge> GetPosCharge(int posChargeId)
        {
            var posChargesQuery = await queryProcessor.Execute(new GetPosChargesAsyncQuery
            {
                Ids = [posChargeId],
                IsCaptured = true,

                IncludeMerchant = true,
                IncludeCharge = true,

                PageSize = 1,
                PageIndex = 0,
            });
            return posChargesQuery.Single();
        }

        private async Task<string> CreateSimplifiedInvoice(PosCharge posCharge)
        {
            string note = $"Taxa de conveniência incidente na refeição no estabelecimento \"{posCharge.Merchant!.Name}\"";
            var invoice = await invoiceGateway.CreateSimplifiedInvoice(new SimplifiedInvoice
            {
                Reference = $"CF-SI-${idConverter.ToPublicId(posCharge.Id)}",
                CreatedDateUtc = dateTimeProvider.GetUtcNow(),
                Notes = note,
                SerieCode = invoiceGateway.BuildCompleteSerieCode("QV-CF"),
                PaymentMethodCode = posCharge.Charge!.ChargeMethod.ToString(),
                CustomerVatNumber = posCharge.VatNumber,
                PricesType = Infrastructure.Abstractions.Pos.Invoicing.Models.PriceType.IncludedTaxes,
                Items =
                [
                    new InvoiceItem(InvoiceItemType.Services)
                    {
                        Reference = "ConvenienceFee",
                        Name = "Taxa de Conveniência",
                        TaxPercentage = 23,
                        Quantity = 1,
                        Price = posCharge.SurchargeFeeAmount,
                        DiscountPercentage = 0,
                        CorrelationId = string.Empty,
                    },
                ],
            });
            return invoice.DocumentId!;
        }
    }
}