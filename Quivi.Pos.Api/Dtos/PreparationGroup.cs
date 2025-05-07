namespace Quivi.Pos.Api.Dtos
{
    public class PreparationGroup
    {
        public required string Id { get; init; }
        public required string SessionId { get; init; }
        public bool IsCommited { get; init; }
        public string? Note { get; init; }
        public required IEnumerable<string> OrderIds { get; init; }
        public required IEnumerable<PreparationGroupItem> Items { get; init; }
        public DateTimeOffset CreatedDate { get; init; }
        public DateTimeOffset LastModified { get; init; }
    }
}