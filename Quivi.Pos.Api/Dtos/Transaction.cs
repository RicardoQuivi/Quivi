namespace Quivi.Pos.Api.Dtos
{
    public class Transaction
    {
        public required string Id { get; init; }
        public bool IsSynced { get; init; }
    }
}