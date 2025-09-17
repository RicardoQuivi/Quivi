using Quivi.Domain.Entities.Pos;
using System.Collections;

namespace Quivi.Application.Extensions
{
    public static class SessionExtensions
    {
        private static OrderState[] validOrderStates = [OrderState.Processing, OrderState.Completed, OrderState.Accepted];

        public class ValidSessionOrders : IEnumerable<Order>
        {
            private readonly IEnumerable<Order> orders;

            public ValidSessionOrders(Session session)
            {
                orders = session.Orders?.Where(o => validOrderStates.Contains(o.State)) ?? [];
            }

            public IEnumerator<Order> GetEnumerator() => orders.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => orders.GetEnumerator();

            public IEnumerable<OrderMenuItem> GetValidOrderMenuItems()
            {
                return orders.SelectMany(o => o.OrderMenuItems ?? []).Where(o => o.ParentOrderMenuItemId.HasValue == false) ?? [];
            }
        }

        public static ValidSessionOrders GetValidOrders(this Session session) => new ValidSessionOrders(session);

        public static IEnumerable<OrderMenuItem> GetValidOrderMenuItems(this Session session) => session.GetValidOrders().GetValidOrderMenuItems();
    }
}