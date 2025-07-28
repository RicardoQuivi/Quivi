using Hangfire.States;
using Quivi.Application.Commands.PosChargeSyncAttempts;
using Quivi.Application.Commands.PreparationGroups;
using Quivi.Application.Commands.PrinterNotificationMessages;
using Quivi.Application.Commands.Sessions;
using Quivi.Application.Queries.Orders;
using Quivi.Application.Queries.PosIntegrations;
using Quivi.Domain.Entities.Notifications;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Pos;
using Quivi.Infrastructure.Abstractions.Pos.Commands;
using Quivi.Infrastructure.Abstractions.Pos.Exceptions;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Storage;
using Quivi.Infrastructure.Jobs.Hangfire.Context;
using Quivi.Infrastructure.Jobs.Hangfire.Filters;

namespace Quivi.Application.Pos
{
    public abstract class APosSyncService : IPosSyncService
    {
        private readonly IDictionary<IntegrationType, IPosSyncStrategy> syncStrategies;
        protected readonly IQueryProcessor queryProcessor;
        protected readonly ICommandProcessor commandProcessor;
        protected readonly IBackgroundJobHandler backgroundJobHandler;
        protected readonly IEventService eventService;
        protected readonly IIdConverter idConverter;
        protected readonly IStorageService storageService;

        public APosSyncService(IEnumerable<IPosSyncStrategy> dataSyncStrategies,
                                IQueryProcessor queryProcessor,
                                ICommandProcessor commandProcessor,
                                IBackgroundJobHandler backgroundJobHandler,
                                IEventService eventService,
                                IStorageService storageService,
                                IIdConverter idConverter)
        {
            this.syncStrategies = dataSyncStrategies.ToDictionary(r => r.IntegrationType);
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.backgroundJobHandler = backgroundJobHandler;
            this.eventService = eventService;
            this.storageService = storageService;
            this.idConverter = idConverter;
        }

        public IPosSyncStrategy? Get(IntegrationType dataSyncStrategyType)
        {
            if (syncStrategies.TryGetValue(dataSyncStrategyType, out var result))
                return result;
            return null;
        }

        public async Task NewConsumerBill(int sessionId, int? locationId)
        {
            var integrationQuery = await queryProcessor.Execute(new GetPosIntegrationsAsyncQuery
            {
                SessionIds = [sessionId],
                IncludeMerchant = true,
                IsDeleted = false,
                PageSize = 1,
            });
            var integration = integrationQuery.Single();

            await commandProcessor.Execute(new CreatePrinterNotificationMessageAsyncCommand
            {
                MessageType = NotificationMessageType.ConsumerBill,
                GetContent = () => syncStrategies[integration.IntegrationType].NewConsumerBill(integration, sessionId),
                Criteria = new GetPrinterNotificationsContactsCriteria
                {
                    MerchantIds = [integration.MerchantId],
                    MessageTypes = [NotificationMessageType.ConsumerBill],
                    LocationIds = locationId.HasValue ? [locationId.Value] : null,
                    IsDeleted = false,

                    PageIndex = 0,
                    PageSize = null,
                },
            });
        }

        #region SyncMenu
        public async Task SyncMenuContextualize(IJobContextualizer contextualizer, int posIntegrationId, IEnumerable<int> menuItemIds)
        {
            var integrationsQuery = await queryProcessor.Execute(new GetPosIntegrationsAsyncQuery
            {
                Ids = [posIntegrationId],
                PageIndex = 0,
                PageSize = 1,
            });
            var integration = integrationsQuery.Single();

            contextualizer.PosIntegrationId = integration.Id;
            contextualizer.MerchantId = integration.MerchantId;
        }

        [ContextualizeFilter(nameof(SyncMenuContextualize))]
        [PerIntegrationDistributedLockFilter]
        public async Task SyncMenu(int posIntegrationId, IEnumerable<int>? menuItemsIds)
        {
            var integrationsQuery = await queryProcessor.Execute(new GetPosIntegrationsAsyncQuery
            {
                Ids = [posIntegrationId],
                IsDeleted = false,
                PageSize = 1,
            });
            var integration = integrationsQuery.SingleOrDefault();
            if (integration == null)
                return;

            try
            {
                await syncStrategies[integration.IntegrationType].SyncMenu(integration, menuItemsIds);
            }
            catch (NotSupportedException)
            {

            }
        }
        #endregion

        #region ProcessCharge
        public async Task ProcessChargeContextualizer(IJobContextualizer contextualizer, int chargeId)
        {
            var integrationsQuery = await queryProcessor.Execute(new GetPosIntegrationsAsyncQuery
            {
                ChargeIds = [chargeId],
                PageSize = 1,
            });
            var integration = integrationsQuery.Single();

            contextualizer.PosIntegrationId = integration.Id;
            contextualizer.MerchantId = integration.MerchantId;
        }

        [ContextualizeFilter(nameof(ProcessChargeContextualizer))]
        [PerIntegrationDistributedLockFilter]
        [ConditionalAutomaticRetry(nameof(ShouldFail), Attempts = 3, DelaysInSeconds = [10, 20, 40])]
        public async Task ProcessCharge(int chargeId)
        {
            var integrationsQuery = await queryProcessor.Execute(new GetPosIntegrationsAsyncQuery
            {
                ChargeIds = [chargeId],
                IsDeleted = false,
                PageSize = 1,
            });
            var integration = integrationsQuery.SingleOrDefault();
            if (integration == null)
                throw new Exception($"Charge Id {chargeId} has an invalid configuration.");

            var states = await commandProcessor.Execute(new UpsertPosChargeSyncAttemptAsyncCommand
            {
                Criteria = new GetPosChargeSyncAttemptsCriteria
                {
                    PosChargeIds = [chargeId],
                    PageSize = 1,
                },
                CreateData =new UpsertPosChargeSyncAttemptAsyncCommand.OnCreateData
                {
                    PosChargeId = chargeId,
                    MerchantId = integration.MerchantId,
                },
                UpdateAction = e =>
                {
                    if (e.State == SyncAttemptState.Failed)
                        e.State = SyncAttemptState.Syncing;

                    return Task.CompletedTask;
                },
            });
            var state = states.Single();
            if (state.State == SyncAttemptState.Synced)
                return;

            try
            {
                var events = await syncStrategies[integration.IntegrationType].ProcessCharge(integration, chargeId);
                await commandProcessor.Execute(new DispatchEventsAsyncCommand
                {
                    Events = events,
                });
            }
            catch (Exception)
            {
                await commandProcessor.Execute(new UpsertPosChargeSyncAttemptAsyncCommand
                {
                    Criteria = new GetPosChargeSyncAttemptsCriteria
                    {
                        Ids = [state.Id],
                        PageSize = 1,
                    },
                    UpdateAction = e =>
                    {
                        if (e.State != SyncAttemptState.Synced)
                            e.State = SyncAttemptState.Failed;

                        return Task.CompletedTask;
                    },
                });
                throw;
            }
        }

        public static bool ShouldFail(FailedState state)
        {
            Type[] allowedExceptions = [ typeof(PosUnavailableException) ];
            return !allowedExceptions.Contains(state.Exception.GetType());
        }
        #endregion

        #region ProcessRefund
        public async Task<bool> CanRefundCharge(int chargeId, decimal amountToRefund, InvoiceRefundType invoiceRefundType)
        {
            if (invoiceRefundType == InvoiceRefundType.Cancellation)
            {
                var integrationsQuery = await queryProcessor.Execute(new GetPosIntegrationsAsyncQuery
                {
                    ChargeIds = [chargeId],
                    IsDeleted = false,
                    PageSize = 1,
                });
                var integration = integrationsQuery.SingleOrDefault();
                if (integration == null)
                    throw new Exception($"Charge Id {chargeId} has an invalid configuration.");

                var dataSyncStrategy = syncStrategies[integration.IntegrationType];
                if (!dataSyncStrategy.ImplementsRefundChargeAsCancellation)
                    return false;
            }

            return true;
        }

        public async Task RefundChargeContextualizer(IJobContextualizer contextualizer, int chargeId, decimal amountToRefund)
        {
            var integrationsQuery = await queryProcessor.Execute(new GetPosIntegrationsAsyncQuery
            {
                ChargeIds = [chargeId],
                PageSize = 1,
            });
            var integration = integrationsQuery.Single();

            contextualizer.PosIntegrationId = integration.Id;
            contextualizer.MerchantId = integration.MerchantId;
        }

        [ContextualizeFilter(nameof(RefundChargeContextualizer))]
        [PerIntegrationDistributedLockFilter]
        public Task RefundCharge(int chargeId, decimal amountToRefund)
        {
            return Task.CompletedTask;
        }
        #endregion

        #region SetUp And Teardown
        public Task OnIntegrationSetUp(PosIntegration integration)
        {
            if (integration.DeletedDate.HasValue)
                return Task.CompletedTask;

            return syncStrategies[integration.IntegrationType].OnIntegrationSetUp(integration);
        }

        public async Task OnIntegrationTearDown(PosIntegration integration)
        {
            if (integration.DeletedDate.HasValue)
                return;

            await syncStrategies[integration.IntegrationType].OnIntegrationTearDown(integration);
            await commandProcessor.Execute(new CloseOpenedSessionsAsyncCommand
            {
                PosIntegrationId = integration.Id,
            });
        }
        #endregion

        #region ProcessOrder
        public async Task<string?> ProcessOrders(IEnumerable<int> orderIds, int merchantId, OrderState fromState, bool complete)
        {
            var ordersQuery = await queryProcessor.Execute(new GetOrdersAsyncQuery
            {
                Ids = orderIds,
                MerchantIds = [merchantId],
                IncludeChannelProfilePosIntegration = true,
                States = [fromState],
                PageIndex = 0,
                PageSize = null,
            });

            if (ordersQuery.Any() == false)
                return null;

            var integrationPerOrders = ordersQuery.Where(o => o.Channel?.ChannelProfile?.PosIntegration?.DeletedDate.HasValue == false)
                                                    .GroupBy(o => o.Channel!.ChannelProfile!.PosIntegration!)
                                                    .ToDictionary(o => o.Key, o => o.AsEnumerable());


            var jobId = string.Empty;
            foreach (var entry in integrationPerOrders)
            {
                var integration = entry.Key;
                var relatedOrderIds = entry.Value.Select(o => o.Id).ToList();
                if (string.IsNullOrEmpty(jobId))
                {
                    jobId = backgroundJobHandler.Enqueue(() => ProcessOrdersJob(relatedOrderIds, integration.MerchantId, integration.Id, fromState, complete));
                    continue;
                }
                jobId = backgroundJobHandler.ContinueJobWith(jobId, () => ProcessOrdersJob(relatedOrderIds, integration.MerchantId, integration.Id, fromState, complete));
            }
            return jobId;
        }


        public void ProcessOrdersContextualizer(IJobContextualizer contextualizer, IEnumerable<int> orderIds, int merchantId, int posIntegrationId, OrderState fromState, bool complete)
        {
            contextualizer.MerchantId = merchantId;
            contextualizer.PosIntegrationId = posIntegrationId;
        }

        [ContextualizeFilter(nameof(ProcessOrdersContextualizer))]
        [PerIntegrationDistributedLockFilter]
        public async Task ProcessOrdersJob(IEnumerable<int> orderIds, int merchantId, int posIntegrationId, OrderState fromState, bool complete)
        {
            var integrationsQuery = await queryProcessor.Execute(new GetPosIntegrationsAsyncQuery
            {
                Ids = [posIntegrationId],
                PageIndex = 0,
                PageSize = 1,
                IsDeleted = false,
            });
            var integration = integrationsQuery.SingleOrDefault();

            if (integration == null)
                throw new Exception($"Pos Integration with Id {posIntegrationId} has an invalid configuration.");

            try
            {
                var strategy = syncStrategies[integration.IntegrationType];
                var events = await strategy.ProcessOrders(integration, orderIds, fromState, complete);
                await commandProcessor.Execute(new DispatchEventsAsyncCommand
                { 
                    Events = events,
                });
                if (new[] { OrderState.Draft, OrderState.Accepted, OrderState.Scheduled }.Contains(fromState))
                    backgroundJobHandler.Enqueue(() => UpdatePreparationGroup(merchantId, orderIds));
            }
            catch (NotSupportedException)
            {
            }
        }

        public void UpdatePreparationGroupContextualizer(IJobContextualizer contextualizer, int merchantId, IEnumerable<int> orderIds)
        {
            contextualizer.MerchantId = merchantId;
        }

        [ContextualizeFilter(nameof(UpdatePreparationGroupContextualizer))]
        [PerMerchantDistributedLockFilter]
        public Task UpdatePreparationGroup(int merchantId, IEnumerable<int> orderIds) => commandProcessor.Execute(new UpsertPreparationGroupAsyncCommand
        {
            MerchantId = merchantId,
            OrderIds = orderIds,
        });
        #endregion

        #region CancelOrder
        public async Task CancelOrderContextualizer(IJobContextualizer contextualizer, int orderId, int merchantId, string reason)
        {
            var integrationsQuery = await queryProcessor.Execute(new GetPosIntegrationsAsyncQuery
            {
                OrderIds = [orderId],
                MerchantIds = [merchantId],
                PageSize = 1,
            });
            var integration = integrationsQuery.Single();

            contextualizer.PosIntegrationId = integration.Id;
            contextualizer.MerchantId = integration.MerchantId;
        }

        [ContextualizeFilter(nameof(CancelOrderContextualizer))]
        [PerIntegrationDistributedLockFilter]
        [ConditionalAutomaticRetry(nameof(ShouldFail), Attempts = 3, DelaysInSeconds = new[] { 10, 20, 40 })]
        public async Task CancelOrder(int orderId, int merchantId, string? reason)
        {
            var integrationsQuery = await queryProcessor.Execute(new GetPosIntegrationsAsyncQuery
            {
                MerchantIds = [merchantId],
                OrderIds = [orderId],
                IsDeleted = false,
                PageIndex = 0,
                PageSize = 1,
            });
            var integration = integrationsQuery.SingleOrDefault();

            if (integration == null)
                throw new Exception($"Pos Integration related with Order Id {orderId} and merchant {merchantId} has an invalid configuration.");

            try
            {
                var events = await syncStrategies[integration.IntegrationType].CancelOrder(integration, orderId, reason);
                await commandProcessor.Execute(new DispatchEventsAsyncCommand
                {
                    Events = events,
                });
            }
            catch (NotSupportedException)
            {
            }
        }
        #endregion
    }
}
