﻿using Quivi.Domain.Entities.Pos;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetMerchantInvoiceDocumentsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? PosChargeIds { get; init; }
        public IEnumerable<InvoiceDocumentType>? Types { get; init; }
        public IEnumerable<DocumentFormat>? Formats { get; init; }
        public IEnumerable<string>? DocumentReferences { get; init; }
        public IEnumerable<string>? DocumentIds { get; init; }
        public bool? HasDownloadPath { get; init; }
        public bool? HasPosCharge { get; init; }
        public bool IncludePosCharge { get; init; }
        public bool IncludePosChargeMerchant { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}