using Quivi.Infrastructure.Abstractions.Pos;

namespace Quivi.Application.Pos
{
    public interface IQuiviSyncSettings : ISyncSettings
    {
        /// <summary>
        /// Indicates if we should skip invoices creation.
        /// </summary>
        bool SkipInvoice { get; }

        /// <summary>
        /// Consider Tip as an item of invoices.
        /// </summary>
        bool IncludeTipInInvoice { get; }

        /// <summary>
        /// A prefix to add to generated invoices.
        /// </summary>
        string InvoicePrefix { get; }
    }
}
