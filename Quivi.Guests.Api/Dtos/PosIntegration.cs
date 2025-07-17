namespace Quivi.Guests.Api.Dtos
{
    public enum PosIntegrationState
    {
        Running = 0,
        Offline = -1,
        Error = -2,
    }

    public class PosIntegration
    {
        public required string Id { get; set; }
        public bool AllowsInvoiceDownloads { get; init; }
        public bool AllowsEscPosInvoices { get; init; }
        public bool AllowsOpeningSessions { get; init; }
        public bool AllowsRemovingItemsFromSession { get; init; }
        public bool AllowsAddingItemsToSession { get; init; }
        public bool AllowsMenuSyncing { get; init; }
        public bool AllowsPayments { get; init; }
        public PosIntegrationState State { get; init; }
        public bool IsActive { get; init; }
    }
}