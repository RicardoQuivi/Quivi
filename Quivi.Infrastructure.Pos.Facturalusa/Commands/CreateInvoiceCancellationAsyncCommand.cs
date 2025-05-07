using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;
using Quivi.Infrastructure.Pos.Facturalusa.Mappers;
using Quivi.Infrastructure.Pos.Facturalusa.Models.External;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Sales;

namespace Quivi.Infrastructure.Pos.Facturalusa.Commands
{
    public class CreateInvoiceCancellationAsyncCommand : AFacturalusaAsyncCommand<InvoiceCancellation>
    {
        public CreateInvoiceCancellationAsyncCommand(IFacturalusaService facturalusaService) : base(facturalusaService)
        {

        }

        public required InvoiceCancellation Data { get; set; }

        public Models.Sales.DownloadSaleFormat? IncludePdfFileInFormat { get; set; }
    }

    public class CreateInvoiceCancellationAsyncCommandHandler : AGetOrCreateSaleCommandHandler, ICommandHandler<CreateInvoiceCancellationAsyncCommand, Task<InvoiceCancellation>>
    {
        public CreateInvoiceCancellationAsyncCommandHandler(IFacturalusaCacheProvider cacheProvider) : base(cacheProvider)
        {
        }

        public async Task<InvoiceCancellation> Handle(CreateInvoiceCancellationAsyncCommand command)
        {
            var findResponse = await command.FacturalusaService.GetSale(new Models.Sales.GetSalesRequest
            {
                FilterBy = Models.Sales.SaleFilter.DocumentNumber,
                Value = command.Data.RelatedDocumentId,
            });

            var result = SaleMapper.ConvertToInvoiceCancellation(findResponse.Data.CanceledAt.HasValue ? new CancelSaleResponse
            {
                Status = true,
                UrlFile = findResponse.Data.PdfFileUrl,
            } : await CancelInvoice(command, findResponse.Data.Id));

            result.DocumentId = command.Data.RelatedDocumentId;
            result.RelatedDocumentId = command.Data.RelatedDocumentId;
            return result;
        }

        private static async Task<CancelSaleResponse> CancelInvoice(CreateInvoiceCancellationAsyncCommand command, long documentId)
        {
            return await command.FacturalusaService.CancelSale(documentId, new Models.Sales.CancelSaleRequest
            {
                Reason = command.Data.Reason.PadRight(15, '.'),
            });
        }
    }
}
