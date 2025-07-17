using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class OrderMapperHandler : IMapperHandler<Order, Dtos.Order>
    {
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;

        public OrderMapperHandler(IMapper mapper, IIdConverter idConverter)
        {
            this.mapper = mapper;
            this.idConverter = idConverter;
        }

        public Dtos.Order Map(Order model)
        {
            return new Dtos.Order
            {
                Id = idConverter.ToPublicId(model.Id),
                SequenceNumber = model.OrderSequence?.SequenceNumber.ToString() ?? idConverter.ToPublicId(model.Id),
                ChannelId = idConverter.ToPublicId(model.ChannelId),
                EmployeeId = model.EmployeeId.HasValue == false ? null : idConverter.ToPublicId(model.EmployeeId.Value),
                State = model.State,
                IsTakeAway = model.OrderType == OrderType.TakeAway,
                OrderOrigin = model.Origin,
                Items = mapper.Map<IEnumerable<OrderMenuItem>, IEnumerable<Dtos.SessionItem>>(model.OrderMenuItems)!,
                Fields = [],
                CreatedDate = new DateTimeOffset(model.CreatedDate, TimeSpan.Zero),
                LastModified = new DateTimeOffset(model.ModifiedDate, TimeSpan.Zero),
            };
        }
    }
}