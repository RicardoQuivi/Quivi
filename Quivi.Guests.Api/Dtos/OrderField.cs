namespace Quivi.Guests.Api.Dtos
{
    public class OrderField
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? DefaultValue { get; set; }
        public bool IsRequired { get; set; }
        public OrderFieldType Type { get; set; }
    }
}