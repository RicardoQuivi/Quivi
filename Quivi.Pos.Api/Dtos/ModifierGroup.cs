namespace Quivi.Pos.Api.Dtos
{
    public class ModifierGroup
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public int MinSelection { get; init; }
        public int MaxSelection { get; init; }
        public required IEnumerable<ModifierGroupOption> Options { get; init; }
    }
}