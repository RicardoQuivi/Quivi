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
        DocumentFormat Format { get; }

        string? DocumentId { get; set; }
        string? DownloadUrl { get; set; }
    }

    public class UpsertMerchantInvoiceDocumentAsyncCommand : AUpdateAsyncCommand<IEnumerable<MerchantInvoiceDocument>, IUpdatableMerchantInvoiceDocument>
    {
        public int MerchantId { get; init; }
        public int? PosChargeId { get; init; }
        public InvoiceDocumentType DocumentType { get; init; }
        public required IEnumerable<DocumentFormat> Formats { get; init; }
        public string? DocumentReference { get; init; }
    }

    public class UpsertMerchantInvoiceDocumentAsyncCommandHandler : ICommandHandler<UpsertMerchantInvoiceDocumentAsyncCommand, Task<IEnumerable<MerchantInvoiceDocument>>>
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
            public DocumentFormat Format => entity.Format;

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

        public async Task<IEnumerable<MerchantInvoiceDocument>> Handle(UpsertMerchantInvoiceDocumentAsyncCommand command)
        {
            var entityQuery = await repository.GetAsync(new GetMerchantInvoiceDocumentsCriteria
            {
                MerchantIds = [command.MerchantId],
                PosChargeIds = command.PosChargeId.HasValue ? [command.PosChargeId.Value] : null,
                DocumentReferences = string.IsNullOrWhiteSpace(command.DocumentReference) ? null : [command.DocumentReference],
                Types = [command.DocumentType],
                Formats = command.Formats,
            });
            var entitiesDictionary = entityQuery.ToDictionary(p => p.Format, p => p);

            var now = dateTimeProvider.GetUtcNow();
            List<MerchantInvoiceDocument> updatedEntities = new List<MerchantInvoiceDocument>();
            List<MerchantInvoiceDocument> newEntities = new List<MerchantInvoiceDocument>();
            foreach (var format in command.Formats)
            {
                bool isNew = false;
                entitiesDictionary.TryGetValue(format, out var entity);
                if (entity == null)
                {
                    entity = new MerchantInvoiceDocument
                    {
                        MerchantId = command.MerchantId,
                        ChargeId = command.PosChargeId,
                        DocumentReference = command.DocumentReference,
                        Format = format,
                        DocumentType = command.DocumentType,
                        CreatedDate = now,
                        ModifiedDate = now,
                    };
                    repository.Add(entity);
                    newEntities.Add(entity);
                    isNew = true;
                }

                var updatableEntity = new UpdatableMerchantInvoiceDocument(entity);
                await command.UpdateAction(updatableEntity);
                if (updatableEntity.HasChanges == false)
                    continue;

                entity.ModifiedDate = now;
                if(isNew == false)
                    updatedEntities.Add(entity);
            }

            if (newEntities.Any() == false && updatedEntities.Any() == false)
                return [];

            await repository.SaveChangesAsync();

            foreach (var entity in updatedEntities)
                await this.eventService.Publish(new OnMerchantInvoiceDocumentOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = entity.MerchantId,
                    PosChargeId = entity.ChargeId,
                    Operation = EntityOperation.Update,
                });

            foreach (var entity in newEntities)
                await this.eventService.Publish(new OnMerchantInvoiceDocumentOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = entity.MerchantId,
                    PosChargeId = entity.ChargeId,
                    Operation = EntityOperation.Create,
                });

            return newEntities.Concat(updatedEntities);
        }
    }
}
