using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class LocationMapperHandler : IMapperHandler<Location, Dtos.Location>
    {
        private readonly IIdConverter idConverter;

        public LocationMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public Dtos.Location Map(Location model)
        {
            return new Dtos.Location
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.Name,
            };
        }
    }
}