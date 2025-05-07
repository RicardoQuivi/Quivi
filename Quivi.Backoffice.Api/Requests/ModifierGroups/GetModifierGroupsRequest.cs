namespace Quivi.Backoffice.Api.Requests.ModifierGroups
{
    public class GetModifierGroupsRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; set; }
        public IEnumerable<string>? MenuItemIds { get; set; }
    }
}
