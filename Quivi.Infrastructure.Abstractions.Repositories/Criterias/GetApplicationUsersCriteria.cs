namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public class GetApplicationUsersCriteria : IPagedCriteria
    {
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }

        public bool IncludeMerchants { get; set; }

        public int PageIndex { get; set; }
        public int? PageSize { get; set; }
    }
}
