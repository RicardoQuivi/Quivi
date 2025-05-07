namespace Quivi.Pos.Api.Dtos
{
    public class PosIntegration
    {
        public required string Id { get; init; }
        public bool IsOnline { get; init; }
        public bool AllowsPayments { get; init; }
        public bool AllowsOpeningSessions { get; init; }
        public bool AllowsEscPosInvoices { get; init; }
        public bool AllowsAddingItemsToSession { get; init; }
        public bool AllowsRemovingItemsFromSession { get; init; }
    }
}