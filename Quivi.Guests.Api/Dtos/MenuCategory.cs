namespace Quivi.Guests.Api.Dtos
{
    public class MenuCategory
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? ImageUrl { get; set; }
    }
}