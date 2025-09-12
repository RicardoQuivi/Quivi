using Quivi.Domain.Entities.Pos;

namespace Quivi.Application.Extensions
{
    public static class SessionExtensions
    {
        public static IEnumerable<OrderMenuItem> GetValidOrderMenuItems(this Session session)
        {
            OrderState[] validOrderStates = [OrderState.Processing, OrderState.Completed, OrderState.Accepted];

            return session.Orders?.Where(o => validOrderStates.Contains(o.State)).SelectMany(o => o.OrderMenuItems ?? []).Where(o => o.ParentOrderMenuItemId.HasValue == false) ?? [];
        }
    }
}