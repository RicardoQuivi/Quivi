using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Merchants;
using Quivi.Infrastructure.Abstractions.Configurations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.MapperHandlers
{
    public class MerchantMapperHandler : IMapperHandler<Merchant, Dtos.Merchant>
    {
        private readonly IIdConverter idConverter;
        private readonly IDefaultSettings defaultSettings;

        public MerchantMapperHandler(IIdConverter idConverter, IDefaultSettings defaultSettings)
        {
            this.idConverter = idConverter;
            this.defaultSettings = defaultSettings;
        }

        public Dtos.Merchant Map(Merchant model)
        {
            return new Dtos.Merchant
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.Name,
                LogoUrl = model.LogoUrl ?? defaultSettings.DefaultMerchantLogo,

                SurchargeFeesMayApply = model.SurchargeFee > 0 || model.SurchargeFees?.Any(f => f.Fee > 0) == true,
                FreePayment = !model.DisabledFeatures.HasFlag(MerchantFeature.FreePayment),
                ItemSelectionPayment = !model.DisabledFeatures.HasFlag(MerchantFeature.ItemSelectionPayment),
                SplitBillPayment = !model.DisabledFeatures.HasFlag(MerchantFeature.SplitBillPayment),
                EnforceTip = !model.DisabledFeatures.HasFlag(MerchantFeature.EnforceTip),
                ShowPaymentNotes = model.DisabledFeatures.HasFlag(MerchantFeature.HidePaymentNotes),
                AllowsIgnoreBill = model.DisabledFeatures.HasFlag(MerchantFeature.DisallowNotMyBill),
                Fees = MapFees(model),
                Inactive = model.DeletedDate.HasValue,
            };
        }

        private IEnumerable<Dtos.Fee> MapFees(Merchant model)
        {
            var surchargeFeesOverrides = model.SurchargeFees?.ToDictionary(f => f.ChargeMethod, f => f) ?? [];
            foreach(var chargeMethod in Enum.GetValues(typeof(ChargeMethod)).Cast<ChargeMethod>())
            {
                surchargeFeesOverrides.TryGetValue(chargeMethod, out var fee);
                var dtoFee = new Dtos.Fee
                {
                    Type = Dtos.FeeType.Surcharge,
                    PaymentMethod = chargeMethod,
                    Unit = fee?.FeeUnit ?? model.SurchargeFeeUnit,
                    Value = fee?.Fee ?? model.SurchargeFee,
                };
                yield return dtoFee;
            }
        }
    }
}