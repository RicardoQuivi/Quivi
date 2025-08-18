namespace FacturaLusa.v2.Dtos.Requests.Sales
{
    public class SearchSaleRequest
    {
        public required string Value { get; init; }
        public SearchField Field { get; init; }
    }
}