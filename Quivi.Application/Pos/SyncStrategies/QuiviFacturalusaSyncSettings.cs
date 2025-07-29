using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Pos;

namespace Quivi.Application.Pos.SyncStrategies
{
    public class QuiviFacturalusaSyncSettings : AConfiguration, ISyncSettings, IQuiviSyncSettings
    {
        public QuiviFacturalusaSyncSettings(PosIntegration configuration) : base(configuration.ConnectionString)
        {
            MerchantId = configuration.MerchantId;
            AccessToken = ExtractOptionalString(ConnectionParameters, nameof(AccessToken)) ?? string.Empty;
            SkipInvoice = ExtractOptionalBoolean(ConnectionParameters, nameof(SkipInvoice)) ?? true;
            IncludeTipInInvoice = ExtractOptionalBoolean(ConnectionParameters, nameof(IncludeTipInInvoice)) ?? false;
            InvoicePrefix = ExtractOptionalString(ConnectionParameters, nameof(InvoicePrefix)) ?? string.Empty;
        }

        public string AccessToken { get; set; }
        public bool SkipInvoice { get; set; }
        public bool IncludeTipInInvoice { get; set; }
        public string InvoicePrefix { get; set; }

        public int MerchantId { get; }

        #region ISyncSettings
        public TimeSpan SyncInterval => TimeSpan.Zero;
        public bool IsRealTime => true;
        public bool AllowsMenuSyncing => true;
        public bool AllowsAddingItemsToSession => true;
        public bool AllowsRemovingItemsFromSession => true;
        public bool AllowsInvoiceDownloads => !SkipInvoice;
        public bool AllowsEscPosInvoices => !SkipInvoice;
        public bool AllowsOpeningSessions => true;
        public bool AllowsPayments => true;
        #endregion
    }
}
