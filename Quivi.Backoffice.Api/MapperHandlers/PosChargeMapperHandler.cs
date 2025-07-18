using Quivi.Backoffice.Api.Dtos;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class PosChargeMapperHandler : IMapperHandler<PosCharge, Dtos.Transaction>
    {
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;

        public PosChargeMapperHandler(IIdConverter idConverter, IMapper mapper)
        {
            this.idConverter = idConverter;
            this.mapper = mapper;
        }

        public Transaction Map(PosCharge model)
        {
            if (model.Charge == null)
                throw new Exception($"{nameof(Charge)} was not included in {nameof(PosCharge)}");

            return new Transaction
            {
                Id = idConverter.ToPublicId(model.ChargeId),
                MerchantId = idConverter.ToPublicId(model.MerchantId),
                CapturedDate = new DateTimeOffset(model.CaptureDate!.Value, TimeSpan.Zero),
                ChargeMethod = model.Charge.ChargeMethod,
                Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email,
                VatNumber = string.IsNullOrWhiteSpace(model.VatNumber) ? null : model.VatNumber,
                Payment = model.Payment,
                PaymentDiscount = GetDiscountTotal(model),
                Tip = model.Tip,
                Surcharge = model.SurchargeFeeAmount,
                RefundedAmount = model.TotalRefund ?? 0.0m,
                InvoiceRefundType = model.InvoiceRefundType,
                SyncingState = GetSyncingState(model),
                SessionId = model.SessionId.HasValue ? idConverter.ToPublicId(model.SessionId.Value) : null,
                ChannelId = idConverter.ToPublicId(model.ChannelId),
                CustomChargeMethodId = model.Charge.MerchantCustomCharge == null ? null : idConverter.ToPublicId(model.Charge.MerchantCustomCharge.CustomChargeMethodId),
                Items = mapper.Map<Dtos.TransactionItem>(model.PosChargeInvoiceItems!),
            };
        }

        private decimal? GetDiscountTotal(PosCharge model)
        {
            if (model.PosChargeInvoiceItems?.Any() != true)
                return null;

            return model.PosChargeInvoiceItems.Where(x => !x.ParentPosChargeInvoiceItemId.HasValue)
                                                .Select(item => new
                                                {
                                                    Quantity = item.Quantity,
                                                    Discount = item.OrderMenuItem!.OriginalPrice - item.OrderMenuItem.FinalPrice,
                                                    Modifiers = item.ChildrenPosChargeInvoiceItems?.Select(m => new
                                                    {
                                                        Quantity = m.Quantity * item.Quantity,
                                                        Discount = m.OrderMenuItem!.OriginalPrice - m.OrderMenuItem.FinalPrice,
                                                    }),
                                                })
                                                .Sum(item => item.Quantity * item.Discount + (item.Modifiers?.Sum(m => m.Quantity * m.Discount) ?? 0));
        }

        private SynchronizationState GetSyncingState(PosCharge model)
        {
            if (model.PosChargeSyncAttempts?.Any() != true)
                return SynchronizationState.Syncing;

            if (model.PosChargeSyncAttempts.Any(c => c.State == SyncAttemptState.Synced))
                return SynchronizationState.Succeeded;

            if (model.PosChargeSyncAttempts.Any(c => c.State == SyncAttemptState.Failed))
                return SynchronizationState.Failed;

            return SynchronizationState.Syncing;
        }
    }
}
