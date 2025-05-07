using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Events;

namespace Quivi.Infrastructure.Abstractions.Pos
{
    public interface IPosSyncStrategy
    {
        IntegrationType IntegrationType { get; }

        ISyncSettings ParseSyncSettings(PosIntegration integration);

        bool ImplementsRefundChargeAsCancellation { get; }

        Task SyncMenu(PosIntegration integration, IEnumerable<int>? digitalMenuItemIds = null);

        #region Order
        Task<IEnumerable<IEvent>> ProcessOrders(PosIntegration integration, IEnumerable<int> orderIds, OrderState fromState, bool complete);
        Task<IEnumerable<IEvent>> CancelOrder(PosIntegration integration, int orderId, string reason);
        #endregion

        #region Charge
        Task<IEnumerable<IEvent>> ProcessCharge(PosIntegration integration, int chargeId);
        Task<decimal> RefundChargeAsCreditNote(PosIntegration integration, int chargeId, decimal amountToRefund);
        Task<decimal> RefundChargeAsCancellation(PosIntegration integration, int chargeId, decimal amountToRefund, string reason);
        #endregion

        Task<byte[]> GetInvoice(PosIntegration integration, int chargeId);
        Task<string> GetEscPosInvoice(PosIntegration integration, int chargeId);
        
        //TODO: Add this?
        //Task<SessionBill> NewConsumerBill(PosIntegration integration, int sessionId);

        Task OnIntegrationSetUp(PosIntegration integration);
        Task OnIntegrationTearDown(PosIntegration integration);
    }

    public interface IPosSyncStrategy<T> : IPosSyncStrategy where T : ISyncSettings
    {
        new T ParseSyncSettings(PosIntegration integration);
    }
}
