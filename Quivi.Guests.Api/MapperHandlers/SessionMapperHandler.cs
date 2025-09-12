using Quivi.Application.Extensions;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.MapperHandlers
{
    public class SessionMapperHandler : IMapperHandler<Session, Dtos.Session>
    {
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;

        public SessionMapperHandler(IMapper mapper, IIdConverter idConverter)
        {
            this.mapper = mapper;
            this.idConverter = idConverter;
        }

        public Dtos.Session Map(Session model)
        {
            return new Dtos.Session
            {
                Id = idConverter.ToPublicId(model.Id),
                ChannelId = idConverter.ToPublicId(model.ChannelId),
                Items = mapper.Map<IEnumerable<OrderMenuItem>, IEnumerable<Dtos.SessionItem>>(model.GetValidOrderMenuItems())!,
            };
        }
    }
}