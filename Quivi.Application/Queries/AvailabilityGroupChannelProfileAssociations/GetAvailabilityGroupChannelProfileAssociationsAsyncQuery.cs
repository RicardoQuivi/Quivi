using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.AvailabilityGroupChannelProfileAssociations
{
    public class GetAvailabilityGroupChannelProfileAssociationsAsyncQuery : APagedAsyncQuery<AvailabilityProfileAssociation>
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? AvailabilityGroupIds { get; init; }
        public IEnumerable<int>? ChannelProfileIds { get; init; }
    }

    public class GetAvailabilityGroupChannelProfileAssociationsAsyncQueryHandler : APagedQueryAsyncHandler<GetAvailabilityGroupChannelProfileAssociationsAsyncQuery, AvailabilityProfileAssociation>
    {
        private readonly IAvailabilityProfileAssociationsRepository repository;

        public GetAvailabilityGroupChannelProfileAssociationsAsyncQueryHandler(IAvailabilityProfileAssociationsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<AvailabilityProfileAssociation>> Handle(GetAvailabilityGroupChannelProfileAssociationsAsyncQuery query)
        {
            return repository.GetAsync(new GetAvailabilityProfileAssociationsCriteria
            {
                MerchantIds = query.MerchantIds,
                AvailabilityGroupIds = query.AvailabilityGroupIds,
                ChannelProfileIds = query.ChannelProfileIds,

                PageSize = query.PageSize,
                PageIndex = query.PageIndex,
            });
        }
    }
}