using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class CustomChargeMethodMapperHandler : IMapperHandler<CustomChargeMethod, Dtos.CustomChargeMethod>
    {
        private readonly IIdConverter idConverter;

        public CustomChargeMethodMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public Dtos.CustomChargeMethod Map(CustomChargeMethod model)
        {
            return new Dtos.CustomChargeMethod
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.Name,
                LogoUrl = model.Logo,
            };
        }
    }
}