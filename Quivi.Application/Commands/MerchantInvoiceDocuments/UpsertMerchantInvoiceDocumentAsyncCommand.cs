using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.MerchantInvoiceDocuments;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.MerchantInvoiceDocuments
{
    public interface IUpdatableMerchantInvoiceDocument : IUpdatableEntity
    {
        int MerchantId { get; }
        int? ChargeId { get; }
        string? DocumentReference { get; set; }
        InvoiceDocumentType DocumentType { get; }

        string? DocumentId { get; set; }
        string? DownloadUrl { get; set; }
    }

    public class UpsertMerchantInvoiceDocumentAsyncCommand : AUpdateAsyncCommand<MerchantInvoiceDocument, IUpdatableMerchantInvoiceDocument>
    {
        public int MerchantId { get; set; }
        public int? PosChargeId { get; set; }
        public InvoiceDocumentType DocumentType { get; set; }
        public string? DocumentReference { get; set; }
    }

    public class UpsertMerchantInvoiceDocumentAsyncCommandHandler : ICommandHandler<UpsertMerchantInvoiceDocumentAsyncCommand, Task<MerchantInvoiceDocument>>
    {
        private readonly IMerchantInvoiceDocumentsRepository repository;
        private readonly IEventService eventService;
        private readonly IDateTimeProvider dateTimeProvider;

        public UpsertMerchantInvoiceDocumentAsyncCommandHandler(IMerchantInvoiceDocumentsRepository repository,
                                                                    IEventService eventService,
                                                                    IDateTimeProvider dateTimeProvider)
        {
            this.repository = repository;
            this.eventService = eventService;
            this.dateTimeProvider = dateTimeProvider;
        }

        private class UpdatableMerchantInvoiceDocument : IUpdatableMerchantInvoiceDocument
        {
            private readonly MerchantInvoiceDocument entity;

            private readonly string? originalDocumentId;
            private readonly string? originalDocumentUrl;
            private readonly string? originalDocumentReference;

            public UpdatableMerchantInvoiceDocument(MerchantInvoiceDocument entity)
            {
                this.entity = entity;
                this.originalDocumentId = entity.DocumentId;
                this.originalDocumentUrl = entity.Path;
                this.originalDocumentReference = entity.DocumentReference;
            }

            public int MerchantId => entity.MerchantId;

            public int? ChargeId => entity.ChargeId;

            public InvoiceDocumentType DocumentType => entity.DocumentType;

            public string? DocumentReference
            {
                get => entity.DocumentReference;
                set => entity.DocumentReference = value;
            }

            public string? DocumentId
            {
                get => entity.DocumentId;
                set => entity.DocumentId = value;
            }
            public string? DownloadUrl
            {
                get => entity.Path;
                set => entity.Path = value;
            }

            public bool HasChanges
            {
                get
                {
                    if (entity.Id == 0)
                        return true;

                    if (originalDocumentId != entity.DocumentId)
                        return true;

                    if (originalDocumentUrl != entity.Path)
                        return true;

                    if (originalDocumentReference != entity.DocumentReference)
                        return true;

                    return false;
                }
            }
        }

        public async Task<MerchantInvoiceDocument> Handle(UpsertMerchantInvoiceDocumentAsyncCommand command)
        {
            var entityQuery = await repository.GetAsync(new GetMerchantInvoiceDocumentsCriteria
            {
                MerchantIds = [command.MerchantId],
                PosChargeIds = command.PosChargeId.HasValue ? [command.PosChargeId.Value] : null,
                DocumentReferences = string.IsNullOrWhiteSpace(command.DocumentReference) ? null : [command.DocumentReference],
                Types = [command.DocumentType],
            });
            var entity = entityQuery.SingleOrDefault();

            var now = dateTimeProvider.GetUtcNow();
            bool isUpdate = true;
            if (entity == null)
            {
                isUpdate = false;
                entity = new MerchantInvoiceDocument
                {
                    MerchantId = command.MerchantId,
                    ChargeId = command.PosChargeId,
                    DocumentReference = command.DocumentReference,
                    DocumentType = command.DocumentType,
                    CreatedDate = now,
                    ModifiedDate = now,
                };
                repository.Add(entity);
            }

            var updatableEntity = new UpdatableMerchantInvoiceDocument(entity);
            await command.UpdateAction(updatableEntity);
            if (updatableEntity.HasChanges == false)
                return entity;

            entity.ModifiedDate = now;
            await repository.SaveChangesAsync();

            await this.eventService.Publish(new OnMerchantInvoiceDocumentOperationEvent
            {
                Id = entity.Id,
                MerchantId = entity.MerchantId,
                PosChargeId = entity.ChargeId,
                Operation = isUpdate ? EntityOperation.Update : EntityOperation.Create,
            });
            return entity;
        }
    }
}
