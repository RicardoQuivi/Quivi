namespace Quivi.Backoffice.Api.Requests.Merchants
{
    public class GetMerchantsRequest : APagedRequest
    {
        public string? Search { get; init; }
        public string? ParentId { get; init; }
        public bool? IsParent { get; init; }
        public IEnumerable<string>? Ids { get; init; }
    }
}
