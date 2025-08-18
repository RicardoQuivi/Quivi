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
        #endregion

        #region Consumer Bill
        Task<ConsumerBill> CreateConsumerBillReceipt(ConsumerBill invoice);
        Task<ConsumerBill> GetConsumerBillReceipt(string documentId);
        Task<byte[]> GetConsumerBillFile(string documentId, DocumentFileFormat format);
        #endregion

        #region Credit Note
        Task<CreditNote> CreateCreditNote(CreditNote creditNote);
        Task<CreditNote> GetCreditNote(string documentId);
        Task<byte[]> GetCreditNoteFile(string documentId, DocumentFileFormat format);
        #endregion

        #region Invoice Cancellation
        Task<InvoiceCancellation> CreateInvoiceCancellation(InvoiceCancellation creditNote);
        #endregion

        #region Simplified Invoice
        Task<SimplifiedInvoice> CreateSimplifiedInvoice(SimplifiedInvoice invoice);
        Task<SimplifiedInvoice> GetSimplifiedInvoice(string documentId);
        Task<byte[]> GetSimplifiedInvoiceFile(string documentId, DocumentFileFormat format);
        #endregion

        Task UpsertInvoiceItems(IEnumerable<ProductItem> items);

        Task<bool> HealthCheck();
    }
}
