using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.PosChargeInvoiceItems
{
    public class GetPosChargeInvoiceItemsAsyncQuery : APagedAsyncQuery<PosChargeInvoiceItem>
    {
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? PosChargeIds { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public bool? IsParent { get; init; }

        public bool IncludeOrderMenuItem { get; init; }
        public bool IncludePosChargeChargeInvoiceDocuments { get; init; }
        public bool IncludePosChargeChargeMerchantCustomChargeCustomChargeMethod { get; init; }
        public bool IncludeChildrenPosChargeInvoiceItems { get; init; }
    }

    public class GetPosChargeInvoiceItemsAsyncQueryHandler : APagedQueryAsyncHandler<GetPosChargeInvoiceItemsAsyncQuery, PosChargeInvoiceItem>
    {
        private readonly IPosChargeInvoiceItemsRepository repository;

        public GetPosChargeInvoiceItemsAsyncQueryHandler(IPosChargeInvoiceItemsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<PosChargeInvoiceItem>> Handle(GetPosChargeInvoiceItemsAsyncQuery query)
        {
            return repository.GetAsync(new GetPosChargeInvoiceItemCriteria
            {
                ParentMerchantIds = query.ParentMerchantIds,
                MerchantIds = query.MerchantIds,
                PosChargeIds = query.PosChargeIds,
                FromDate = query.FromDate,
                ToDate = query.ToDate,
                IsParent = query.IsParent,

                IncludeOrderMenuItem = query.IncludeOrderMenuItem,
                IncludePosChargeChargeInvoiceDocuments = query.IncludePosChargeChargeInvoiceDocuments,
                IncludePosChargeChargeMerchantCustomChargeCustomChargeMethod = query.IncludePosChargeChargeMerchantCustomChargeCustomChargeMethod,
                IncludeChildrenPosChargeInvoiceItems = query.IncludeChildrenPosChargeInvoiceItems,

                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}
