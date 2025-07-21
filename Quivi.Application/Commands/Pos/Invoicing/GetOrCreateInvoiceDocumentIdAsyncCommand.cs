using Quivi.Application.Commands.MerchantInvoiceDocuments;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Services;

namespace Quivi.Application.Commands.Pos.Invoicing
{
    public class GetOrCreateInvoiceDocumentIdAsyncCommand : ICommand<Task<string>>
    {
        public required int MerchantId { get; init; }
        public int? PosChargeId { get; init; }
        public required InvoiceDocumentType DocumentType { get; init; }
        public string? InternalReference { get; init; }
        public required CreateDocumentAction CreateNewDocumentIdAction { get; init; }
    }

    public delegate Task<string> CreateDocumentAction();

    public class GetOrCreateInvoiceDocumentIdAsyncCommandHandler : ICommandHandler<GetOrCreateInvoiceDocumentIdAsyncCommand, Task<string>>
    {
        private readonly ICommandProcessor commandProcessor;
        private readonly IBackgroundJobHandler backgroundJobHandler;
        private readonly ILogger logger;

        public GetOrCreateInvoiceDocumentIdAsyncCommandHandler(ICommandProcessor commandProcessor,
                                                                IBackgroundJobHandler backgroundJobHandler,
                                                                ILogger logger)
        {
            this.commandProcessor = commandProcessor;
            this.backgroundJobHandler = backgroundJobHandler;
            this.logger = logger;
        }

        public async Task<string> Handle(GetOrCreateInvoiceDocumentIdAsyncCommand command)
        {
            var result = await commandProcessor.Execute(new UpsertMerchantInvoiceDocumentAsyncCommand
            {
                MerchantId = command.MerchantId,
                PosChargeId = command.PosChargeId,
                DocumentType = command.DocumentType,
                DocumentReference = command.InternalReference,
                UpdateAction = async (e) =>
                {
                    if (string.IsNullOrWhiteSpace(e.DocumentId) == false)
                        return;

                    string? documentId = null;
                    try
                    {
                        // Invoke external method to create new DocumentId
                        documentId = await command.CreateNewDocumentIdAction();
                        if (documentId == null)
                            return;

                        e.DocumentId = documentId;
                    }
                    catch (Exception ex)
                    {
                        // If create doc fails then propagate the exception
                        if (documentId == null)
                            throw;

                        logger.LogException(ex);
                        backgroundJobHandler.Enqueue(() => TryToSaveLostInvoiceDocumentId(command.MerchantId, command.PosChargeId, command.DocumentType, documentId, command.InternalReference));
                    }
                },
            });
            return result.DocumentId!;
        }

        public Task TryToSaveLostInvoiceDocumentId(int merchantId, int? chargeId, InvoiceDocumentType documentType, string documentId, string? internalReference)
        {
            return commandProcessor.Execute(new UpsertMerchantInvoiceDocumentAsyncCommand
            {
                MerchantId = merchantId,
                PosChargeId = chargeId,
                DocumentType = documentType,
                DocumentReference = internalReference,
                UpdateAction = (e) =>
                {
                    if (string.IsNullOrWhiteSpace(e.DocumentId) == false)
                        return Task.CompletedTask;

                    e.DocumentId = documentId;
                    return Task.CompletedTask;
                },
            });
        }
    }
}
