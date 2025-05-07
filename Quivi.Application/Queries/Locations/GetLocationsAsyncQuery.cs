using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.Locations
{
    public class GetLocationsAsyncQuery : APagedAsyncQuery<Location>
    {
        public IEnumerable<int>? MerchantIds { get; set; }
        public IEnumerable<int>? Ids { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class GetLocationsAsyncQueryHandler : APagedQueryAsyncHandler<GetLocationsAsyncQuery, Location>
    {
        private readonly ILocationsRepository repository;

        public GetLocationsAsyncQueryHandler(ILocationsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<Location>> Handle(GetLocationsAsyncQuery query)
        {
            return repository.GetAsync(new GetLocationsCriteria
            {
                Ids = query.Ids,
                MerchantIds = query.MerchantIds,
                IsDeleted = query.IsDeleted,

                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}
