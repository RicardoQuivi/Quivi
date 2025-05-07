using Quivi.Domain.Entities.Merchants;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.Merchants
{
    public class GetMerchantsAsyncQuery : APagedAsyncQuery<Merchant>
    {
        public IEnumerable<int>? ApplicationUserIds { get; set; }
        public IEnumerable<int>? Ids { get; set; }
        public IEnumerable<int>? ParentIds { get; set; }
        public IEnumerable<int>? ChildIds { get; set; }
        public string? Search { get; init; }
        public bool? Inactive { get; init; }
        public bool? IsParentMerchant { get; init; }

        public bool IncludeChildMerchants { get; init; }
        public bool IncludeParentMerchant { get; init; }
    }

    public class GetMerchantsAsyncQueryHandler : APagedQueryAsyncHandler<GetMerchantsAsyncQuery, Merchant>
    {
        private readonly IMerchantsRepository repository;

        public GetMerchantsAsyncQueryHandler(IMerchantsRepository repository) 
        {
            this.repository = repository;
        }

        public override Task<IPagedData<Merchant>> Handle(GetMerchantsAsyncQuery query)
        {
            return repository.GetAsync(new GetMerchantsCriteria
            {
                Ids = query.Ids,
                ParentIds = query.ParentIds,
                ChildIds = query.ChildIds,
                ApplicationUserIds = query.ApplicationUserIds,
                IsParentMerchant = query.IsParentMerchant,
                Search = query.Search,
                Inactive = query.Inactive,

                IncludeChildMerchants = query.IncludeChildMerchants,
                IncludeParentMerchant = query.IncludeParentMerchant,

                PageSize = query.PageSize,
                PageIndex = query.PageIndex,
            });
        }
    }
}
