using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlMerchantInvoiceDocumentsRepository : ARepository<MerchantInvoiceDocument, GetMerchantInvoiceDocumentsCriteria>, IMerchantInvoiceDocumentsRepository
    {
        public SqlMerchantInvoiceDocumentsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<MerchantInvoiceDocument> GetFilteredQueryable(GetMerchantInvoiceDocumentsCriteria criteria)
        {
            IQueryable<MerchantInvoiceDocument> query = Set;

            if (criteria.Ids != null)
                query = query.Where(q => criteria.Ids.Contains(q.Id));

            if (criteria.MerchantIds != null)
                query = query.Where(q => criteria.MerchantIds.Contains(q.MerchantId));

            if (criteria.PosChargeIds != null)
                query = query.Where(q => q.ChargeId.HasValue && criteria.PosChargeIds.Contains(q.ChargeId.Value));

            if (criteria.Types != null)
                query = query.Where(q => criteria.Types.Contains(q.DocumentType));

            if (criteria.DocumentReferences != null)
                query = query.Where(q => criteria.DocumentReferences.Contains(q.DocumentReference));

            if (criteria.DocumentIds != null)
                query = query.Where(q => criteria.DocumentIds.Contains(q.DocumentId));

            return query.OrderByDescending(x => x.CreatedDate);
        }
    }
}