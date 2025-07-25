﻿using Quivi.Domain.Entities.Pos;

namespace Quivi.Infrastructure.Abstractions.Pos
{
    public interface IPosSyncService
    {
        IPosSyncStrategy? Get(IntegrationType dataSyncStrategyType);
        Task SyncMenu(int posIntegrationId, IEnumerable<int>? digitalMenuItemIds = null);

        #region Order
        Task<string?> ProcessOrders(IEnumerable<int> orderIds, int merchantId, OrderState fromState, bool complete);
        Task CancelOrder(int orderId, int merchantId, string? reason);
        #endregion

        #region Charge
        Task<bool> CanRefundCharge(int chargeId, decimal amountToRefund, InvoiceRefundType invoiceRefundType);
        Task ProcessCharge(int chargeId);
        Task RefundCharge(int chargeId, decimal amountToRefund);
        #endregion

        #region Documents
        Task NewConsumerBill(int sessionId, int? locationId);
        #endregion

        Task OnIntegrationSetUp(PosIntegration integration);
        Task OnIntegrationTearDown(PosIntegration integration);
    }
}
