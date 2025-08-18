namespace FacturaLusa.v2.Dtos.Requests.Currencies
{
    public class CreateCurrencyRequest
    {
        public required string Description { get; init; }
        public required string Symbol { get; init; }
        public required string IsoCode { get; init; }
        public required decimal ExchangeSale { get; init; }
        public bool IsDefault { get; init; }
    }
}