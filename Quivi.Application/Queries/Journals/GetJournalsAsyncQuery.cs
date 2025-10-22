using Quivi.Domain.Entities.Financing;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.Journals
{
    public class GetJournalsAsyncQuery : APagedAsyncQuery<Journal>
    {
        public IEnumerable<JournalState>? States { get; init; }
        public IEnumerable<JournalType>? Types { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public IEnumerable<string>? OrderRefs { get; init; }

        public bool IncludeJournalDetails { get; init; }
        public bool IncludeMerchantFees { get; init; }
        public bool IncludeSubMerchantFees { get; init; }
    }

    internal class GetJournalsAsyncQueryHandler : APagedQueryAsyncHandler<GetJournalsAsyncQuery, Journal>
    {
        private readonly IJournalsRepository journalsRepository;

        public GetJournalsAsyncQueryHandler(IJournalsRepository journalsRepository)
        {
            this.journalsRepository = journalsRepository;
        }

        public override Task<IPagedData<Journal>> Handle(GetJournalsAsyncQuery query)
        {
            return journalsRepository.GetAsync(new GetJournalsCriteria
            {
                States = query.States,
                Types = query.Types,
                FromDate = query.FromDate,
                ToDate = query.ToDate,
                OrderRefs = query.OrderRefs,

                IncludeJournalDetails = query.IncludeJournalDetails,
                IncludeMerchantFees = query.IncludeMerchantFees,
                IncludeSubMerchantFees = query.IncludeSubMerchantFees,

                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}
