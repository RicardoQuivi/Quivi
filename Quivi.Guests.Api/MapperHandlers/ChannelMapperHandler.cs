using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.MapperHandlers
{
    public class ChannelMapperHandler : IMapperHandler<Channel, Dtos.Channel>
    {
        private readonly IIdConverter idConverter;

        public ChannelMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public Dtos.Channel Map(Channel model)
        {
            return new Dtos.Channel
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.Identifier,
                ChannelProfileId = idConverter.ToPublicId(model.ChannelProfileId),
                MerchantId = idConverter.ToPublicId(model.MerchantId),
            };
        }
    }
}
