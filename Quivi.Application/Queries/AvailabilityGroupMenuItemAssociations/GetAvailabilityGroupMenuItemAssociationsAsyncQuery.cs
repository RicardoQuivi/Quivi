using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.AvailabilityGroupMenuItemAssociations
{
    public class GetAvailabilityGroupMenuItemAssociationsAsyncQuery : APagedAsyncQuery<AvailabilityMenuItemAssociation>
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? AvailabilityGroupIds { get; init; }
        public IEnumerable<int>? MenuItemIds { get; init; }
    }

    public class GetAvailabilityGroupMenuItemAssociationsAsyncQueryHandler : APagedQueryAsyncHandler<GetAvailabilityGroupMenuItemAssociationsAsyncQuery, AvailabilityMenuItemAssociation>
    {
        private readonly IAvailabilityMenuItemAssociationsRepository repository;

        public GetAvailabilityGroupMenuItemAssociationsAsyncQueryHandler(IAvailabilityMenuItemAssociationsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<AvailabilityMenuItemAssociation>> Handle(GetAvailabilityGroupMenuItemAssociationsAsyncQuery query)
        {
            return repository.GetAsync(new GetAvailabilityMenuItemAssociationsCriteria
            {
                MerchantIds = query.MerchantIds,
                AvailabilityGroupIds = query.AvailabilityGroupIds,
                MenuItemIds = query.MenuItemIds,

                PageSize = query.PageSize,
                PageIndex = query.PageIndex,
            });
        }
    }
}