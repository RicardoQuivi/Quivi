namespace Quivi.Guests.Api.Dtos.Requests.MenuCategories
{
    public class GetMenuCategoriesRequest : APagedRequest
    {
        public required string ChannelId { get; set; }
        public DateTimeOffset? AtDate { get; set; }
    }
}