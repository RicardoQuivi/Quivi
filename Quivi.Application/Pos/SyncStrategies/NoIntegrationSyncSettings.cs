using Quivi.Infrastructure.Abstractions.Pos;

namespace Quivi.Application.Pos.SyncStrategies
{
    public class NoIntegrationSyncSettings : ISyncSettings
    {
        public TimeSpan SyncInterval => TimeSpan.Zero;
        public bool IsRealTime => false;
        public bool AllowsInvoiceDownloads => false;
        public bool AllowsEscPosInvoices => false;
        public bool AllowsOpeningSessions => false;
        public bool AllowsRemovingItemsFromSession => false;
        public bool AllowsAddingItemsToSession => false;
        public bool AllowsMenuSyncing => false;
        public bool AllowsPayments => false;
    }
}