using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.CustomChargeMethods
{
    public class GetCustomChargeMethodsAsyncQuery : APagedAsyncQuery<CustomChargeMethod>
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<string>? Names { get; init; }
    }

    public class GetCustomChargeMethodsAsyncQueryHandler : APagedQueryAsyncHandler<GetCustomChargeMethodsAsyncQuery, CustomChargeMethod>
    {
        private readonly ICustomChargeMethodsRepository repository;

        public GetCustomChargeMethodsAsyncQueryHandler(ICustomChargeMethodsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<CustomChargeMethod>> Handle(GetCustomChargeMethodsAsyncQuery query)
        {
            return repository.GetAsync(new GetCustomChargeMethodsCriteria
            {
                Ids = query.Ids,
                MerchantIds = query.MerchantIds,
                Names = query.Names,

                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}