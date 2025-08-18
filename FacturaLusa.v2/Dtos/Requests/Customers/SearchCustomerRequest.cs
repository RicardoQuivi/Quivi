namespace FacturaLusa.v2.Dtos.Requests.Customers
{
    public class SearchCustomerRequest
    {
        public required string Value { get; init; }
        public SearchField? SearchField { get; init; }
    }
}