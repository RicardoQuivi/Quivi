using Quivi.Application.Commands.MerchantInvoiceDocuments;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Services;
using Quivi.Infrastructure.Abstractions.Storage;

namespace Quivi.Application.Commands.Pos.Invoicing
{
    public record CreateDocumentResponse
    {
        public required string DocumentId { get; init; }
        public byte[]? FileData { get; init; }
    }

    public class GetOrCreateInvoiceDocumentIdAsyncCommand : ICommand<Task>
    {
        public required int MerchantId { get; init; }
        public required int PosChargeId { get; init; }
        public required InvoiceDocumentType DocumentType { get; init; }
        public string? InternalReference { get; init; }
        public CreateDocumentAction? CreatePdfDocument { get; init; }
        public CreateDocumentAction? CreateEscPosDocument { get; init; }
    }

    public delegate Task<CreateDocumentResponse> CreateDocumentAction();

    public class GetOrCreateInvoiceDocumentIdAsyncCommandHandler : ICommandHandler<GetOrCreateInvoiceDocumentIdAsyncCommand, Task>
    {
        private readonly ICommandProcessor commandProcessor;
        private readonly IBackgroundJobHandler backgroundJobHandler;
        private readonly IIdConverter idConverter;
        private readonly IStorageService storageService;
        private readonly ILogger logger;

        public GetOrCreateInvoiceDocumentIdAsyncCommandHandler(ICommandProcessor commandProcessor,
                                                                IBackgroundJobHandler backgroundJobHandler,
                                                                ILogger logger,
                                                                IIdConverter idConverter,
                                                                IStorageService storageService)
        {
            this.commandProcessor = commandProcessor;
            this.backgroundJobHandler = backgroundJobHandler;
            this.logger = logger;
            this.idConverter = idConverter;
            this.storageService = storageService;
        }

        public async Task Handle(GetOrCreateInvoiceDocumentIdAsyncCommand command)
        {
            Dictionary<DocumentFormat, CreateDocumentAction> formats = new Dictionary<DocumentFormat, CreateDocumentAction>();

            if (command.CreateEscPosDocument != null)
                formats.Add(DocumentFormat.EscPos, command.CreateEscPosDocument);

            if (command.CreatePdfDocument != null)
                formats.Add(DocumentFormat.Pdf, command.CreatePdfDocument);

            if (formats.Any() == false)
                return;

            var result = await commandProcessor.Execute(new UpsertMerchantInvoiceDocumentAsyncCommand
            {
                MerchantId = command.MerchantId,
                PosChargeId = command.PosChargeId,
                DocumentType = command.DocumentType,
                Formats = formats.Keys,
                DocumentReference = command.InternalReference,
                UpdateAction = async (e) =>
                {
                    if (string.IsNullOrWhiteSpace(e.DocumentId) == false)
                        return;

                    CreateDocumentResponse? result = null;
                    try
                    {
                        result = await formats[e.Format]();

                        e.DocumentId = result.DocumentId;
                        e.DownloadUrl = await StoreFile(command.MerchantId, result.DocumentId, e.Format, result.FileData);
                    }
                    catch (Exception ex)
                    {
                        // If create doc fails then propagate the exception
                        if (result == null)
                            throw;

                        logger.LogException(ex);
                        backgroundJobHandler.Enqueue(() => TryToSaveLostInvoiceDocumentId(command.MerchantId, command.PosChargeId, command.DocumentType, e.Format, result, command.InternalReference));
                    }
                },
            });
        }

        private async Task<string?> StoreFile(int merchantId, string documentId, DocumentFormat format, byte[]? fileData)
        {
            if (fileData == null)
                return null;

            using (var stream = new MemoryStream(fileData))
            {
                var url = await storageService.SaveFile(stream, $"{documentId}.{(format == DocumentFormat.Pdf ? "pdf" : string.Empty)}", idConverter.ToPublicId(merchantId));
                return url;
            }
        }

        public Task TryToSaveLostInvoiceDocumentId(int merchantId, int chargeId, InvoiceDocumentType documentType, DocumentFormat format, CreateDocumentResponse document, string? internalReference)
        {
            return commandProcessor.Execute(new UpsertMerchantInvoiceDocumentAsyncCommand
            {
                MerchantId = merchantId,
                PosChargeId = chargeId,
                DocumentType = documentType,
                Formats = [format],
                DocumentReference = internalReference,
                UpdateAction = async (e) =>
                {
                    e.DocumentId = document.DocumentId;
                    e.DownloadUrl = await StoreFile(merchantId, document.DocumentId, format, document.FileData);
                },
            });
        }
    }
}