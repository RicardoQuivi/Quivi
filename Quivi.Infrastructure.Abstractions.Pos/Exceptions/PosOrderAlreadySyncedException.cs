using Quivi.Domain.Entities.Pos;

namespace Quivi.Infrastructure.Abstractions.Pos.Exceptions
{
    public class PosOrderAlreadySyncedException : PosException
    {
        public int OrderId { get; }
        public OrderState CurrentState { get; }
        public OrderState NextState { get; }

        public PosOrderAlreadySyncedException(int orderId, OrderState currentState, OrderState nextState) : base($"Order {orderId} cannot be synced from state {currentState} to state {nextState}")
        {
            OrderId = orderId;
            CurrentState = currentState;
            NextState = nextState;
        }
    }
}
