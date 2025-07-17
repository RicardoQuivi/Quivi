using Quivi.Application.Commands.Pos;
using Quivi.Application.Commands.PosIntegrations;
using Quivi.Application.Queries.PosIntegrations;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Pos;
using Quivi.Infrastructure.Abstractions.Pos.Invoicing;
using Quivi.Infrastructure.Abstractions.Pos.Invoicing.Models;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Jobs.Hangfire.Context;
using Quivi.Infrastructure.Jobs.Hangfire.Filters;
using System.Text;

namespace Quivi.Application.Pos
{
    public abstract class AQuiviSyncStrategy
    {
        public class InvoiceItem
        {
            public InvoiceItemType Type { get; init; }
            public decimal VatRate { get; init; }
            public decimal UnitPrice { get; init; }
            public int MenuItemId { get; init; }
            public required string Name { get; init; }
            public decimal Quantity { get; init; }
            public decimal DiscountPercentage { get; init; }
        }

        public abstract Task ProcessInvoiceJob(int chargeId, decimal paymentAmount, IEnumerable<InvoiceItem> itemsToBePaid);
    }

    public abstract class AQuiviSyncStrategy<T> : AQuiviSyncStrategy, IPosSyncStrategy<T> where T : IQuiviSyncSettings
    {
        protected IQueryProcessor QueryProcessor { get; }
        protected ICommandProcessor CommandProcessor { get; }
        protected IInvoiceGatewayFactory InvoiceGatewayFactory { get; }

        public AQuiviSyncStrategy(ICommandProcessor commandProcessor, IInvoiceGatewayFactory invoiceGatewayFactory, IQueryProcessor queryProcessor)
        {
            CommandProcessor = commandProcessor;
            InvoiceGatewayFactory = invoiceGatewayFactory;
            QueryProcessor = queryProcessor;
        }

        #region Abstract
        public abstract IntegrationType IntegrationType { get; }
        public abstract bool ImplementsRefundChargeAsCancellation { get; }


        ISyncSettings IPosSyncStrategy.ParseSyncSettings(PosIntegration integration) => this.ParseSyncSettings(integration);
        public abstract T ParseSyncSettings(PosIntegration integration);

        public abstract Task SyncMenu(PosIntegration integration, IEnumerable<int>? digitalMenuItemIds = null);
        #endregion

        protected virtual IInvoiceGateway GetInvoiceGateway(T settings)
        {
            if (settings.IsDefault)
                return InvoiceGatewayFactory.GetDefaultInvoiceGateway();
            return InvoiceGatewayFactory.GetInvoiceGateway(settings);
        }

        public virtual Task<byte[]> GetInvoice(PosIntegration integration, int chargeId)
        {
            return Task.FromResult(Array.Empty<byte>());
        }

        public virtual async Task<string> GetEscPosInvoice(PosIntegration integration, int chargeId)
        {
            var settings = ParseSyncSettings(integration);
            var gateway = GetInvoiceGateway(settings);

            string docId = await GetInvoiceReceiptDocumentId(integration.MerchantId, chargeId, gateway);
            var file = await gateway.GetInvoiceReceiptFile(docId, DocumentFileFormat.EscPOS);
            var escPosContentBase64 = Encoding.UTF8.GetString(file);

            return escPosContentBase64;
        }

        //public async Task<SessionBill> NewConsumerBill(PosIntegration integration, int sessionId)
        //{
        //    var settings = ParseSyncSettings(integration);
        //    var gateway = GetInvoiceGateway(settings);
        //    var bill = await CommandProcessor.Execute(new ProcessQuiviSessionConsumerBillAsyncCommand(gateway)
        //    {
        //        SessionId = sessionId,
        //        InvoicePrefix = settings.InvoicePrefix,
        //    });
        //    return bill;
        //}

        protected virtual Task<string> GetInvoiceReceiptDocumentId(int merchantId, int chargeId, IInvoiceGateway invoiceGateway)
        {
            return Task.FromResult(string.Empty);
            //var merchantDocumentQuery = await QueryProcessor.Execute(new GetInvoiceDocumentsAsyncQuery
            //{
            //    MerchantId = merchantId,
            //    ChargeId = chargeId,
            //    Types = [InvoiceDocumentType.OrderInvoice],
            //});

            //var merchantDocument = merchantDocumentQuery.SingleOrDefault();
            //if (string.IsNullOrWhiteSpace(merchantDocument?.DocumentId))
            //    throw new Exception($"The invoice of charge Id {chargeId} has no invoice!");

            //return merchantDocument.DocumentId;
        }

        public virtual async Task OnIntegrationSetUp(PosIntegration integration)
        {
            SyncState state = SyncState.Running;

            var settings = ParseSyncSettings(integration);
            if (settings.SkipInvoice == false)
            {
                var gateway = GetInvoiceGateway(settings);
                bool isHealthy = await gateway.HealthCheck();
                if (!isHealthy)
                    state = SyncState.SyncFailure;
            }

            await CommandProcessor.Execute(new UpdatePosIntegrationsAsyncCommand
            {
                Criteria = new GetPosIntegrationsCriteria
                {
                    Ids = [integration.Id],
                    IsDeleted = false,
                },
                UpdateAction = e =>
                {
                    e.SyncState = state;
                    return Task.CompletedTask;
                },
            });
        }

        public virtual Task OnIntegrationTearDown(PosIntegration integration)
        {
            return CommandProcessor.Execute(new UpdatePosIntegrationsAsyncCommand
            {
                Criteria = new GetPosIntegrationsCriteria
                {
                    Ids = [integration.Id],
                    IsDeleted = false,
                },
                UpdateAction = e =>
                {
                    e.SyncState = SyncState.Unknown;
                    return Task.CompletedTask;
                },
            });
        }

        public virtual Task<IEnumerable<IEvent>> ProcessOrders(PosIntegration integration, IEnumerable<int> orderIds, OrderState fromState, bool complete)
        {
            return CommandProcessor.Execute(new ProcessQuiviOrderAsyncCommand
            {
                OrderIds = orderIds,
                FromState = fromState,
                ToFinalState = complete,
                SyncStrategy = this,
            });
        }

        public virtual Task<IEnumerable<IEvent>> CancelOrder(PosIntegration integration, int orderId, string? reason)
        {
            return CommandProcessor.Execute(new ProcessQuiviOrderCancelationAsyncCommand
            {
                OrderId = orderId,
                Reason = reason,
            });
        }

        public virtual Task<IEnumerable<IEvent>> ProcessCharge(PosIntegration integration, int chargeId)
        {
            return CommandProcessor.Execute(new ProcessQuiviSyncChargeAsyncCommand
            {
                PosChargeId = chargeId,
                SyncStrategy = this,
            });
        }

        public async Task ProcessInvoiceJobContextualizer(IJobContextualizer contextualizer, int chargeId, decimal paymentAmount, IEnumerable<InvoiceItem> itemsToBePaid)
        {
            var integrationsQuery = await QueryProcessor.Execute(new GetPosIntegrationsAsyncQuery
            {
                ChargeIds = [chargeId],
                PageSize = 1,
            });
            var integration = integrationsQuery.Single();

            contextualizer.PosIntegrationId = integration.Id;
            contextualizer.MerchantId = integration.MerchantId;
        }

        [ContextualizeFilter(nameof(ProcessInvoiceJobContextualizer))]
        [PerIntegrationDistributedLockFilter]
        public override async Task ProcessInvoiceJob(int chargeId, decimal paymentAmount, IEnumerable<InvoiceItem> itemsToBePaid)
        {
            var integrationsQuery = await QueryProcessor.Execute(new GetPosIntegrationsAsyncQuery
            {
                ChargeIds = [chargeId],
                IsDeleted = false,
                PageSize = 1,
            });

            var integration = integrationsQuery.Single();
            var settings = ParseSyncSettings(integration);
            if (settings.SkipInvoice || paymentAmount == 0)
                return;

            var gateway = GetInvoiceGateway(settings);
            //await CommandProcessor.Execute(new ProcessQuiviChargeInvoiceAsyncCommand(gateway)
            //{
            //    ChargeId = chargeId,
            //    PaymentAmount = paymentAmount,
            //    InvoiceItems = itemsToBePaid,
            //    IncludeTip = settings.IncludeTipInInvoice,
            //    InvoicePrefix = settings.InvoicePrefix,
            //});
        }

        public virtual Task<decimal> RefundChargeAsCreditNote(PosIntegration integration, int chargeId, decimal amountToRefund)
        {
            var settings = ParseSyncSettings(integration);
            if (settings.SkipInvoice)
                return Task.FromResult(amountToRefund);

            var gateway = GetInvoiceGateway(settings);
            //await CommandProcessor.Execute(new CreateCreditNoteFromChargeAsyncCommand(gateway)
            //{
            //    ChargeId = chargeId,
            //    RefundAmount = amountToRefund,
            //    InvoicePrefix = settings.InvoicePrefix,
            //    IncludeTipInInvoice = settings.IncludeTipInInvoice,
            //});

            return Task.FromResult(amountToRefund);
        }

        public virtual Task<decimal> RefundChargeAsCancellation(PosIntegration integration, int chargeId, decimal amountToRefund, string reason)
        {
            var settings = ParseSyncSettings(integration);
            if (settings.SkipInvoice)
                return Task.FromResult(amountToRefund);

            if (!ImplementsRefundChargeAsCancellation)
                throw new NotImplementedException();

            //var gateway = GetInvoiceGateway(settings);
            //await CommandProcessor.Execute(new CreateInvoiceCancellationFromChargeAsyncCommand(gateway)
            //{
            //    ChargeId = chargeId,
            //    RefundAmount = amountToRefund,
            //    Reason = reason,
            //    InvoicePrefix = settings.InvoicePrefix,
            //});

            return Task.FromResult(amountToRefund);
        }
    }
}
