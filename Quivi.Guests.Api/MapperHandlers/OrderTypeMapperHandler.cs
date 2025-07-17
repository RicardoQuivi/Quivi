using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.MapperHandlers
{
    public class OrderTypeMapperHandler : IMapperHandler<OrderType, Dtos.OrderType>
    {
        public Dtos.OrderType Map(OrderType model)
        {
            switch (model)
            {
                case OrderType.TakeAway: return Dtos.OrderType.TakeAway;
                case OrderType.OnSite: return Dtos.OrderType.OnSite;
            }
            throw new NotImplementedException();
        }
    }
}