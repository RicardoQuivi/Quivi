using Quivi.Domain.Entities.Pos;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetSessionsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? ChannelIds { get; init; }
        public IEnumerable<int>? PreparationGroupIds { get; init; }
        public IEnumerable<int>? PosIntegrationIds { get; init; }
        public IEnumerable<SessionStatus>? Statuses { get; init; }
        public bool LatestSessionsOnly { get; init; }

        public bool IncludePreparationGroups { get; init; }
        public bool IncludePreparationGroupsItems { get; init; }
        public bool IncludeOrders { get; init; }
        public bool IncludeOrdersMenuItems { get; init; }
        public bool IncludeOrdersMenuItemsPosChargeInvoiceItems { get; init; }
        public bool IncludeChannel { get; init; }
        public bool IncludeOrdersMenuItemsModifiers { get; init; }
        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}
