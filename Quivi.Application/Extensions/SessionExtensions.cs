using Quivi.Domain.Entities.Pos;

namespace Quivi.Application.Extensions
{
    public static class SessionExtensions
    {
        private static OrderState[] validOrderStates = [OrderState.Processing, OrderState.Completed, OrderState.Accepted];
        public static IEnumerable<OrderMenuItem> GetValidOrderMenuItems(this Session session)
        {
            return session.Orders?.Where(o => validOrderStates.Contains(o.State)).SelectMany(o => o.OrderMenuItems ?? []).Where(o => o.ParentOrderMenuItemId.HasValue == false) ?? [];
        }
    }
}