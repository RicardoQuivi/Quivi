using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.MapperHandlers
{
    public class OrderChangeLogMapperHandler : IMapperHandler<OrderChangeLog, Dtos.OrderChangeLog>
    {
        private readonly IMapper _mapper;

        public OrderChangeLogMapperHandler(IMapper mapper)
        {
            _mapper = mapper;
        }

        public Dtos.OrderChangeLog Map(OrderChangeLog model)
        {
            return new Dtos.OrderChangeLog
            {
                State = _mapper.Map<Dtos.OrderState>(model.State),
                Note = model.Notes,
                LastModified = new DateTimeOffset(model.ModifiedDate, TimeSpan.Zero),
            };
        }
    }
}