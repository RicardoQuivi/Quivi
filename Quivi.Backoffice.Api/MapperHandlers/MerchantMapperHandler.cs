using Quivi.Domain.Entities.Merchants;
using Quivi.Infrastructure.Abstractions.Configurations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class MerchantMapperHandler : IMapperHandler<Merchant, Dtos.Merchant>
    {
        private readonly IDefaultSettings defaultSettings;
        private readonly IIdConverter idConverter;

        public MerchantMapperHandler(IDefaultSettings defaultSettings, IIdConverter idConverter) 
        {
            this.defaultSettings = defaultSettings;
            this.idConverter = idConverter;
        }

        public Dtos.Merchant Map(Merchant model)
        {
            string? logoUrl = model.LogoUrl;

            if (logoUrl == null)
                logoUrl = defaultSettings.DefaultMerchantLogo;

            return new Dtos.Merchant
            {
                Id = idConverter.ToPublicId(model.Id),
                ParentId = model.ParentMerchantId == null ? null : idConverter.ToPublicId(model.ParentMerchantId.Value),
                Name = model.Name,
                VatNumber = model.VatNumber ?? "",
                LogoUrl = logoUrl,
                SetUpFee = 0.0m,

                TransactionFee = model.TransactionFee,
                TransactionFeeUnit = model.TransactionFeeUnit,

                SurchargeFee = model.SurchargeFee,
                SurchargeFeeUnit = model.SurchargeFeeUnit,

                SurchargeFees = model.SurchargeFees?.ToDictionary(s => s.ChargeMethod, s => new Dtos.MerchantFee
                {
                    Fee = s.Fee,
                    Unit = s.FeeUnit,
                }) ?? [],
            };
        }
    }
}
