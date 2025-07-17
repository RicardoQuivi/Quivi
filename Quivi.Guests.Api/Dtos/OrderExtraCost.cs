namespace Quivi.Guests.Api.Dtos
{
    public class OrderExtraCost
    {
        public ExtraCostType Type { get; init; }
        public decimal Amount { get; init; }
    }
}