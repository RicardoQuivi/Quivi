using Quivi.Application.Queries.MerchantInvoiceDocuments;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Pos;

namespace Quivi.Application.Commands.MerchantInvoiceDocuments
{
    public class PrintMerchantInvoiceDocumentAsyncCommand : ICommand<Task>
    {
        public int MerchantInvoiceDocumentId { get; init; }
    }

    public class PrintMerchantInvoiceDocumentAsyncCommandHandler : ICommandHandler<PrintMerchantInvoiceDocumentAsyncCommand, Task>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IPosSyncService posSyncService;

        public PrintMerchantInvoiceDocumentAsyncCommandHandler(IQueryProcessor queryProcessor, IPosSyncService posSyncService)
        {
            this.queryProcessor = queryProcessor;
            this.posSyncService = posSyncService;
        }

        public async Task Handle(PrintMerchantInvoiceDocumentAsyncCommand command)
        {
            var documentsQuery = await queryProcessor.Execute(new GetMerchantInvoiceDocumentsAsyncQuery
            {
                Ids = [command.MerchantInvoiceDocumentId],
                PageIndex = 0,
                PageSize = 1,
            });

            var document = documentsQuery.SingleOrDefault();
            if (document == null)
                return;

            if (document.ChargeId.HasValue == false)
                throw new Exception("Only printing of merchant invoice documents associated with charges are supported");

            if (document.DocumentType == InvoiceDocumentType.OrderInvoice)
                await posSyncService.NewEscPosInvoice(document.ChargeId.Value);
        }
    }
}
