using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class LocationMapperHandler : IMapperHandler<Location, Dtos.Local>
    {
        private readonly IIdConverter idConverter;

        public LocationMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public Dtos.Local Map(Location model)
        {
            return new Dtos.Local
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.Name,
            };
        }
    }
}
