using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Configurations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class CustomChargeMethodMapperHandler : IMapperHandler<CustomChargeMethod, Dtos.CustomChargeMethod>
    {
        private readonly IDefaultSettings defaultSettings;
        private readonly IIdConverter idConverter;

        public CustomChargeMethodMapperHandler(IDefaultSettings defaultSettings, IIdConverter idConverter)
        {
            this.defaultSettings = defaultSettings;
            this.idConverter = idConverter;
        }

        public Dtos.CustomChargeMethod Map(CustomChargeMethod model)
        {
            return new Dtos.CustomChargeMethod
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.Name,
                LogoUrl = model.Logo ?? defaultSettings.DefaultMerchantLogo,
            };
        }
    }
}