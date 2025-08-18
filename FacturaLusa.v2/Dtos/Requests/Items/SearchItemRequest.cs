namespace FacturaLusa.v2.Dtos.Requests.Items
{
    public class SearchItemRequest
    {
        public required string Value { get; init; }
        public SearchField? Field { get; init; }
    }
}