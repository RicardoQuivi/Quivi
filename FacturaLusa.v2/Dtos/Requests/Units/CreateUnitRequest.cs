namespace FacturaLusa.v2.Dtos.Requests.Units
{
    public class CreateUnitRequest
    {
        public required string Description { get; init; }
        public required string Symbol { get; init; }
    }
}
