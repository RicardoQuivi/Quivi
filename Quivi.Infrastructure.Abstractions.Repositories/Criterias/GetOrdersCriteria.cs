using Quivi.Domain.Entities.Pos;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetOrdersCriteria : IPagedCriteria
    {
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? SessionIds { get; init; }
        public IEnumerable<OrderState>? States { get; init; }
        public bool? AssociatedWithSession { get; set; }
        public bool? AssociatedWithPreparationGroup { get; set; }

        public bool IncludeChangeLogs { get; init; }
        public bool IncludeOrderMenuItems { get; init; }
        public bool IncludeOrderMenuItemsPosChargeInvoiceItems { get; init; }
        public bool IncludeOrderMenuItemsAndMofifiers { get; init; }
        public bool IncludeChannel { get; init; }
        public bool IncludeChannelProfile { get; init; }
        public bool IncludeChannelProfilePosIntegration { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}
