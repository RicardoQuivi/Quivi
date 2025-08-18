using FacturaLusa.v2.Dtos;
using FacturaLusa.v2.Dtos.Requests.Sales;
using FacturaLusa.v2.Dtos.Responses.Sales;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Commands
{
    public class CreateInvoiceCancellationAsyncCommand : AFacturaLusaAsyncCommand<Sale>
    {
        public required CreateInvoiceCancellationData Data { get; init; }
        public DocumentFormat? Format { get; init; }

        public class CreateInvoiceCancellationData
        {
            public required string DocumentId { get; init; }
            public required string Reason { get; init; }
        }
    }

    public class CreateInvoiceCancellationAsyncCommandHandler : AGetOrCreateSaleCommandHandler, ICommandHandler<CreateInvoiceCancellationAsyncCommand, Task<Sale>>
    {
        public CreateInvoiceCancellationAsyncCommandHandler(IFacturaLusaCacheProvider cacheProvider) : base(cacheProvider)
        {
        }

        public async Task<Sale> Handle(CreateInvoiceCancellationAsyncCommand command)
        {
            Sale sale = await command.Service.SearchSale(new SearchSaleRequest
            {
                Field = SearchField.DocumentNumber,
                Value = command.Data.DocumentId,
            });

            if (sale.CanceledAt.HasValue == false)
                await CancelInvoice(command, sale.Id);

            return sale with
            {
                SaleReferenceId = sale.Id,
                SaleReference = new SaleReference
                {
                    Id = sale.Id,
                    DocumentFullNumber = sale.DocumentFullNumber,
                    IssueDate = sale.CanceledAt!.Value,
                    Serie = sale.Serie,
                    SerieId = sale.SerieId,
                    DocumentTypeId = sale.DocumentTypeId,
                    DueDate = sale.DueDate,
                    GrandTotal = sale.GrandTotal,
                },
            };
        }

        private static Task<CancelSaleResponse> CancelInvoice(CreateInvoiceCancellationAsyncCommand command, long saleId)
        {
            return command.Service.CancelSale(new CancelSaleRequest
            {
                SaleId = saleId,
                Reason = command.Data.Reason.PadRight(15, '.'),
            });
        }
    }
}
