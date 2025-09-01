using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Pos.Api.Dtos;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class PosChargeMapperHandler : IMapperHandler<PosCharge, Dtos.Transaction>
    {
        private readonly IIdConverter idConverter;

        public PosChargeMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public Transaction Map(PosCharge model)
        {
            bool isFreePayment = IsFreePayment(model);

            var customChargeMethodId = model.Charge?.MerchantCustomCharge?.CustomChargeMethodId;
            return new Transaction
            {
                Id = idConverter.ToPublicId(model.Id),
                ChannelId = idConverter.ToPublicId(model.ChannelId),
                EmployeeId = model.EmployeeId.HasValue ? idConverter.ToPublicId(model.EmployeeId.Value) : null,
                RefundEmployeeId = model.RefundEmployeeId.HasValue ? idConverter.ToPublicId(model.RefundEmployeeId.Value) : null,
                SessionId = model.SessionId.HasValue ? idConverter.ToPublicId(model.SessionId.Value) : null,
                CustomChargeMethodId = customChargeMethodId.HasValue ? idConverter.ToPublicId(customChargeMethodId.Value) : null,
                Payment = model.Payment,
                Tip = model.Tip,
                RefundedAmount = (model.PaymentRefund ?? 0.0m) + (model.TipRefund ?? 0.0m),
                IsSynced = isFreePayment || model.PosChargeInvoiceItems?.Any() == true,
                IsFreePayment = isFreePayment,
                Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email,
                VatNumber = string.IsNullOrWhiteSpace(model.VatNumber) ? null : model.VatNumber,
                CapturedDate = new DateTimeOffset(model.CaptureDate!.Value, TimeSpan.Zero),
                LastModified = new DateTimeOffset(model.ModifiedDate, TimeSpan.Zero),
            };
        }

        private static bool IsFreePayment(PosCharge model) => model.SessionId.HasValue == false && model.PosChargeInvoiceItems?.Any() == false;
    }
}