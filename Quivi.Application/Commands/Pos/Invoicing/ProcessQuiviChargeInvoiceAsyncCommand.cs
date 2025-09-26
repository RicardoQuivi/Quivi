using Quivi.Application.Extensions.Pos;
using Quivi.Application.Queries.PosCharges;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Pos.Invoicing;
using Quivi.Infrastructure.Abstractions.Pos.Invoicing.Models;

namespace Quivi.Application.Commands.Pos.Invoicing
{
    public class ProcessQuiviChargeInvoiceAsyncCommand : ICommand<Task>
    {
        public required IInvoiceGateway InvoiceGateway { get; init; }
        public required int PosChargeId { get; init; }
        public required bool IncludeTip { get; init; }
        public required string InvoicePrefix { get; init; }
    }

    public class ProcessQuiviChargeInvoiceAsyncCommandHandler : ICommandHandler<ProcessQuiviChargeInvoiceAsyncCommand, Task>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IIdConverter idConverter;
        private readonly IDateTimeProvider dateTimeProvider;

        public ProcessQuiviChargeInvoiceAsyncCommandHandler(IQueryProcessor queryProcessor,
                                                            ICommandProcessor commandProcessor,
                                                            IIdConverter idConverter,
                                                            IDateTimeProvider dateTimeProvider)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.idConverter = idConverter;

            this.dateTimeProvider = dateTimeProvider;
        }

        public async Task Handle(ProcessQuiviChargeInvoiceAsyncCommand command)
        {
            var posCharge = await GetPosCharge(command.PosChargeId);

            Lazy<Task<InvoiceReceipt>> receipt = new Lazy<Task<InvoiceReceipt>>(() => CreateInvoice(command, posCharge));
            await commandProcessor.Execute(new GetOrCreateMerchantInvoiceDocumentIdAsyncCommand
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
        }

        private async Task<PosCharge> GetPosCharge(int id)
        {
            var posChargeQuery = await queryProcessor.Execute(new GetPosChargesAsyncQuery
            {
                Ids = [id],
                IncludePosChargeInvoiceItems = true,
                IncludePosChargeInvoiceItemsOrderMenuItems = true,
                IncludeMerchantCustomCharge = true,
                IncludeMerchantCustomChargeCustomChargeMethod = true,
                PageSize = 1,
            });
            return posChargeQuery.Single();
        }

        private Task<InvoiceReceipt> CreateInvoice(ProcessQuiviChargeInvoiceAsyncCommand command, PosCharge posCharge)
        {

            var invoiceItems = posCharge.PosChargeInvoiceItems!.AsConvertedSessionItems().Select(g =>
            {
                var first = g.Source.First();
                var orderMenuItem = first.OrderMenuItem!;
                var menuItemId = first.OrderMenuItem!.MenuItemId;

                return new InvoiceItem(InvoiceItemType.ProcessedProducts)
                {
                    Reference = idConverter.ToPublicId(orderMenuItem.MenuItemId),
                    CorrelationId = idConverter.ToPublicId(orderMenuItem.MenuItemId),
                    Name = orderMenuItem.Name,
                    TaxPercentage = orderMenuItem.VatRate,
                    Price = g.Price,
                    Quantity = g.Quantity,
                    DiscountPercentage = g.Discount,
                };
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
            return command.InvoiceGateway.CreateInvoiceReceipt(new InvoiceReceipt
            {
                Reference = $"IR-${idConverter.ToPublicId(charge.Id)}",
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
                PricesType = Infrastructure.Abstractions.Pos.Invoicing.Models.PriceType.IncludedTaxes,
                Items = invoiceItems,
            });
        }
    }
}