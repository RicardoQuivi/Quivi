using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.MapperHandlers
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
            return new Dtos.ChannelProfile
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.Name,
                PosIntegrationId = idConverter.ToPublicId(model.PosIntegrationId),
                Features = model.Features,
                PrePaidOrderingMinimumAmount = model.PrePaidOrderingMinimumAmount ?? 0.0m,
            };
        }
    }
}