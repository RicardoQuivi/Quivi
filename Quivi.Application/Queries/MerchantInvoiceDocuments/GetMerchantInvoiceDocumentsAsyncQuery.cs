using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.MerchantInvoiceDocuments
{
    public class GetMerchantInvoiceDocumentsAsyncQuery : APagedAsyncQuery<MerchantInvoiceDocument>
    {
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? PosChargeIds { get; init; }
        public IEnumerable<InvoiceDocumentType>? Types { get; init; }
        public IEnumerable<DocumentFormat>? Formats { get; init; }
        public IEnumerable<string>? DocumentReferences { get; init; }
        public IEnumerable<string>? DocumentIds { get; init; }
        public bool? HasDownloadPath { get; init; }
        public bool? HasPosCharge { get; init; }
        public bool IncludePosCharge { get; init; }
        public bool IncludePosChargeMerchant { get; init; }
    }

    public class GetMerchantInvoiceDocumentsAsyncQueryHandler : IQueryHandler<GetMerchantInvoiceDocumentsAsyncQuery, Task<IPagedData<MerchantInvoiceDocument>>>
    {
        private readonly IMerchantInvoiceDocumentsRepository repository;

        public GetMerchantInvoiceDocumentsAsyncQueryHandler(IMerchantInvoiceDocumentsRepository repository)
        {
            this.repository = repository;
        }

        public Task<IPagedData<MerchantInvoiceDocument>> Handle(GetMerchantInvoiceDocumentsAsyncQuery query)
        {
            return repository.GetAsync(new GetMerchantInvoiceDocumentsCriteria
            {
                Ids = query.Ids,
                ParentMerchantIds = query.ParentMerchantIds,
                MerchantIds = query.MerchantIds,
                PosChargeIds = query.PosChargeIds,
                Types = query.Types,
                Formats = query.Formats,
                DocumentIds = query.DocumentIds,
                DocumentReferences = query.DocumentReferences,
                HasDownloadPath = query.HasDownloadPath,
                HasPosCharge = query.HasPosCharge,
                IncludePosCharge = query.IncludePosCharge,
                IncludePosChargeMerchant = query.IncludePosChargeMerchant,

                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}
