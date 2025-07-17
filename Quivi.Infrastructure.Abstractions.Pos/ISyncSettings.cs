namespace Quivi.Infrastructure.Abstractions.Pos
{
    public interface ISyncSettings
    {
        TimeSpan SyncInterval { get; }
        bool IsRealTime { get; }
        bool AllowsInvoiceDownloads { get; }
        bool AllowsEscPosInvoices { get; }
        bool AllowsOpeningSessions { get; }
        bool AllowsRemovingItemsFromSession { get; }
        bool AllowsAddingItemsToSession { get; }
        bool AllowsMenuSyncing { get; }
        bool AllowsPayments { get; }
    }
}