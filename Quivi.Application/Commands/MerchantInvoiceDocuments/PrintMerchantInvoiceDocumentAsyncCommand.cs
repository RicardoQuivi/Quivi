using Quivi.Application.Commands.PrinterNotificationMessages;
using Quivi.Application.Queries.MerchantInvoiceDocuments;
using Quivi.Domain.Entities.Notifications;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.MerchantInvoiceDocuments
{
    public class PrintMerchantInvoiceDocumentAsyncCommand : ICommand<Task>
    {
        public int MerchantInvoiceDocumentId { get; init; }
    }

    public class ProcessMerchantInvoiceDocumentAsyncCommandHandler : ICommandHandler<PrintMerchantInvoiceDocumentAsyncCommand, Task>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;

        public ProcessMerchantInvoiceDocumentAsyncCommandHandler(IQueryProcessor queryProcessor,
                                                                    ICommandProcessor commandProcessor)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
        }

        public async Task Handle(PrintMerchantInvoiceDocumentAsyncCommand command)
        {
            var documentsQuery = await queryProcessor.Execute(new GetMerchantInvoiceDocumentsAsyncQuery
            {
                Ids = [command.MerchantInvoiceDocumentId],
                Formats = [DocumentFormat.EscPos],
                HasDownloadPath = true,
                PageIndex = 0,
                PageSize = 1,
            });

            var document = documentsQuery.SingleOrDefault();
            if (document == null)
                return;

            if (document.ChargeId.HasValue == false)
                throw new Exception("Only printing of merchant invoice documents associated with charges are supported");

            if (document.DocumentType == InvoiceDocumentType.OrderInvoice)
            {
                await commandProcessor.Execute(new CreatePrinterNotificationMessageAsyncCommand
                {
                    MessageType = NotificationMessageType.NewConsumerInvoice,
                    GetContent = async () =>
                    {
                        using HttpClient client = new HttpClient();
                        return await client.GetStringAsync(document.Path!);
                    },
                    Criteria = new GetPrinterNotificationsContactsCriteria
                    {
                        MerchantIds = [document.MerchantId],
                        MessageTypes = [NotificationMessageType.NewConsumerInvoice],
                        IsDeleted = false,

                        PageIndex = 0,
                        PageSize = null,
                    },
                });
            }
        }
    }
}