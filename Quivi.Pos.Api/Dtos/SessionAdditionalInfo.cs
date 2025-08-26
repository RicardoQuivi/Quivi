namespace Quivi.Pos.Api.Dtos
{
    public class SessionAdditionalInfo
    {
        public required string Id { get; init; }
        public required string Value { get; init; }
        public required string OrderId { get; init; }
    }
}