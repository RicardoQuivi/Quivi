namespace FacturaLusa.v2.Dtos.Requests.Series
{
    public class CreateSerieRequest
    {
        public required string Description { get; init; }
        public long ValidUntilYear { get; init; }
    }
}