using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class ChannelProfileMapperHandler : IMapperHandler<ChannelProfile, Dtos.ChannelProfile>
    {
        private readonly IIdConverter idConverter;

        public ChannelProfileMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public Dtos.ChannelProfile Map(ChannelProfile model)
        {
            var id = idConverter.ToPublicId(model.Id);
            return new Dtos.ChannelProfile
            {
                Id = id,
                Name = model.Name,
                Features = model.Features,
                PosIntegrationId = idConverter.ToPublicId(model.PosIntegrationId),
            };
        }
    }
}
