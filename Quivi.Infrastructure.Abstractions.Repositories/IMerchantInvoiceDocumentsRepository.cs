﻿using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Abstractions.Repositories
{
    public interface IMerchantInvoiceDocumentsRepository : IRepository<MerchantInvoiceDocument, GetMerchantInvoiceDocumentsCriteria>
    {
    }
}