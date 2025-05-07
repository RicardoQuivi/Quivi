using Quivi.Infrastructure.Abstractions.Pos.Invoicing.Models;

namespace Quivi.Infrastructure.Abstractions.Pos.Invoicing
{
    public interface IInvoiceGateway
    {
        /// <summary>
        /// The gateway initials usefull to distinguish gateways.
        /// </summary>
        string GatewayCode { get; }

        #region Invoice Receipt

        Task<InvoiceReceipt> CreateInvoiceReceipt(InvoiceReceipt invoice);

        Task<InvoiceReceipt> GetInvoiceReceipt(string documentId);

        Task<byte[]> GetInvoiceReceiptFile(string documentId, DocumentFileFormat format);

        Task<string> GetInvoiceReceiptFileUrl(string documentId, DocumentFileFormat format);

        #endregion

        #region Consumer Bill

        Task<ConsumerBill> CreateConsumerBillReceipt(ConsumerBill invoice);

        Task<ConsumerBill> GetConsumerBillReceipt(string documentId);

        Task<byte[]> GetConsumerBillFile(string documentId, DocumentFileFormat format);

        Task<string> GetConsumerBillFileUrl(string documentId, DocumentFileFormat format);

        #endregion

        #region Credit Note

        Task<CreditNote> CreateCreditNote(CreditNote creditNote);

        Task<CreditNote> GetCreditNote(string documentId);

        Task<byte[]> GetCreditNoteFile(string documentId, DocumentFileFormat format);

        Task<string> GetCreditNoteFileUrl(string documentId, DocumentFileFormat format);

        #endregion

        #region Invoice Cancellation

        Task<InvoiceCancellation> CreateInvoiceCancellation(InvoiceCancellation creditNote);

        #endregion

        #region Simplified Invoice

        Task<SimplifiedInvoice> CreateSimplifiedInvoice(SimplifiedInvoice invoice);

        Task<SimplifiedInvoice> GetSimplifiedInvoice(string documentId);

        Task<byte[]> GetSimplifiedInvoiceFile(string documentId, DocumentFileFormat format);

        Task<string> GetSimplifiedInvoiceFileUrl(string documentId, DocumentFileFormat format);

        #endregion

        /// <summary>
        /// Insert or Updates if already exists menus items.
        /// </summary>
        /// <param name="items">The menu items to upsert.</param>
        /// <returns></returns>
        Task UpsertInvoiceItems(IEnumerable<ProductItem> items);

        /// <summary>
        /// Checks if the gateway connection is healthy.
        /// </summary>
        /// <returns></returns>
        Task<bool> HealthCheck();
    }
}
