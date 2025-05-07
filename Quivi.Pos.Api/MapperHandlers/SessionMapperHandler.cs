using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Pos.Api.MapperHandlers
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
                EmployeeId = model.EmployeeId == null ? null : idConverter.ToPublicId(model.EmployeeId.Value),
                IsOpen = model.Status == SessionStatus.Ordering,
                IsDeleted = model.Status == SessionStatus.Unknown,
                ClosedAt = model.EndDate.HasValue ? new DateTimeOffset(model.EndDate.Value, TimeSpan.Zero) : null,
                Items = mapper.Map<IEnumerable<OrderMenuItem>, IEnumerable<Dtos.SessionItem>>(model.Orders!.SelectMany(o => o.OrderMenuItems!))!,
            };
        }
    }
}