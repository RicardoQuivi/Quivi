﻿namespace Quivi.Infrastructure.Abstractions.Events.Data.MerchantInvoiceDocuments
{
    public record OnMerchantInvoiceDocumentOperationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int Id { get; init; }
        public int MerchantId { get; init; }
        public int? PosChargeId { get; init; }
    }
}