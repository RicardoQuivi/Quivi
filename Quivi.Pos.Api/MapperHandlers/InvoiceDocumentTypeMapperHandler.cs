using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class InvoiceDocumentTypeMapperHandler : IMapperHandler<InvoiceDocumentType, Dtos.TransactionDocumentType>
    {
        public Dtos.TransactionDocumentType Map(InvoiceDocumentType model)
        {
            switch (model)
            {
                case InvoiceDocumentType.OrderInvoice: return Dtos.TransactionDocumentType.Order;
                case InvoiceDocumentType.SurchargeInvoice: return Dtos.TransactionDocumentType.Surcharge;
                case InvoiceDocumentType.CreditNote: return Dtos.TransactionDocumentType.CreditNote;
                case InvoiceDocumentType.InvoiceCancellation: return Dtos.TransactionDocumentType.Cancellation;
            }
            throw new NotImplementedException();
        }
    }
}