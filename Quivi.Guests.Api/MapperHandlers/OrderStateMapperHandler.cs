using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.MapperHandlers
{
    public class OrderStateMapperHandler : IMapperHandler<OrderState, Dtos.OrderState>,
                                                IMapperHandler<Dtos.OrderState, OrderState>
    {
        public Dtos.OrderState Map(OrderState model)
        {
            switch (model)
            {
                case OrderState.Draft: return Dtos.OrderState.PreOrder;
                case OrderState.Requested: return Dtos.OrderState.Requested;
                case OrderState.Rejected: return Dtos.OrderState.Rejected;
                case OrderState.Processing: return Dtos.OrderState.Processing;
                case OrderState.Completed: return Dtos.OrderState.Completed;
                case OrderState.ScheduledRequested: return Dtos.OrderState.Requested;
                case OrderState.Scheduled: return Dtos.OrderState.Scheduled;
            }
            throw new NotImplementedException();
        }

        public OrderState Map(Dtos.OrderState model)
        {
            switch (model)
            {
                case Dtos.OrderState.PreOrder: return OrderState.Draft;
                case Dtos.OrderState.Requested: return OrderState.Requested;
                case Dtos.OrderState.Rejected: return OrderState.Rejected;
                case Dtos.OrderState.Processing: return OrderState.Processing;
                case Dtos.OrderState.Completed: return OrderState.Completed;
                case Dtos.OrderState.Scheduled: return OrderState.Scheduled;
            }
            throw new NotImplementedException();
        }
    }
}