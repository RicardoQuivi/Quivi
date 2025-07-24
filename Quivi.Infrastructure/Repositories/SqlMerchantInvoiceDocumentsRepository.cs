using Microsoft.EntityFrameworkCore;
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

            if (criteria.IncludePosCharge)
                query = query.Include(q => q.Charge!).ThenInclude(q => q.PosCharge);

            if (criteria.IncludePosChargeMerchant)
                query = query.Include(q => q.Charge!).ThenInclude(q => q.PosCharge!).ThenInclude(q => q.Merchant);

            if (criteria.Ids != null)
                query = query.Where(q => criteria.Ids.Contains(q.Id));

            if (criteria.MerchantIds != null)
                query = query.Where(q => criteria.MerchantIds.Contains(q.MerchantId));

            if (criteria.PosChargeIds != null)
                query = query.Where(q => q.ChargeId.HasValue && criteria.PosChargeIds.Contains(q.ChargeId.Value));

            if (criteria.Types != null)
                query = query.Where(q => criteria.Types.Contains(q.DocumentType));

            if (criteria.Formats != null)
                query = query.Where(q => criteria.Formats.Contains(q.Format));

            if (criteria.DocumentReferences != null)
                query = query.Where(q => criteria.DocumentReferences.Contains(q.DocumentReference));

            if (criteria.DocumentIds != null)
                query = query.Where(q => criteria.DocumentIds.Contains(q.DocumentId));

            if (criteria.HasDownloadPath.HasValue)
                query = query.Where(q => string.IsNullOrWhiteSpace(q.Path) != criteria.HasDownloadPath.Value);

            if (criteria.HasPosCharge.HasValue)
                query = query.Where(q => q.ChargeId.HasValue == criteria.HasPosCharge.Value);

            return query.OrderByDescending(x => x.CreatedDate);
        }
    }
}