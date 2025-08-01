using Quivi.Application.Commands.Charges;
using Quivi.Application.Commands.PosPosCharges;
using Quivi.Application.Extensions;
using Quivi.Application.Queries.Orders;
using Quivi.Application.Queries.PosCharges;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.PosCharges;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Pos;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using static Quivi.Application.Extensions.PosChargeExtensions;

namespace Quivi.Hangfire.EventHandlers.PosCharges
{
    public class PosChargeEventHandler : BackgroundEventHandler<OnPosChargeOperationEvent>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IPosSyncService posSyncService;

        public PosChargeEventHandler(IQueryProcessor queryProcessor,
                                        ICommandProcessor commandProcessor,
                                        IPosSyncService posSyncService,
                                        IBackgroundJobHandler backgroundJobHandler) : base(backgroundJobHandler)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.posSyncService = posSyncService;
        }

        public override async Task Run(OnPosChargeOperationEvent message)
        {
            var posChargesQuery = await queryProcessor.Execute(new GetPosChargesAsyncQuery
            {
                Ids = [message.Id],
                PageIndex = 0,
                PageSize = 1,
                IncludeCharge = true,
                IncludePosChargeSelectedMenuItems = true,
            });
            var posCharge = posChargesQuery.Single();

            if (posCharge.CaptureDate.HasValue)
            {
                await ProcessCaptured(posCharge);
                ProcessChainedCharge(posCharge);
                return;
            }

            if (message.Operation == EntityOperation.Create && posCharge.CaptureDate.HasValue == false)
            {
                this.backgroundJobHandler.Schedule(() => ExpireCharge(message.Id), new DateTimeOffset(posCharge.CreatedDate.AddMinutes(10), TimeSpan.Zero));
                return;
            }
        }

        private async Task ProcessCaptured(PosCharge posCharge)
        {
            switch (posCharge.GetPosChargeType())
            {
                case PosChargeType.FreePayment: return;
                case PosChargeType.PayAtTheTable: ValidateAndSyncCharge(posCharge); break;
                case PosChargeType.OrderAndPay: await ValidateAndSyncOrder(posCharge); break;
            }
        }

        private void ProcessChainedCharge(PosCharge posCharge)
        {
            if (posCharge.Charge!.ChainedChargeId.HasValue == false)
                return;

            //TODO: Implement the logic to process the chained charge
            //await commandProcessor.Execute(new ProcessChainedChargeAsyncCommand
            //{
            //    ChargeId = posCharge.Charge.ChainedChargeId.Value,
            //});
        }

        private void ValidateAndSyncCharge(PosCharge posCharge)
        {
            if (posCharge.SessionId.HasValue == false)
                throw new Exception($"PosCharge with Id {posCharge.Id} has no session Id.");

            backgroundJobHandler.Enqueue(() => posSyncService.ProcessCharge(posCharge.Id));
        }

        private async Task ValidateAndSyncOrder(PosCharge posCharge)
        {
            if (posCharge.SessionId.HasValue)
            {
                ValidateAndSyncCharge(posCharge);
                return;
            }

            var orderQuery = await queryProcessor.Execute(new GetOrdersAsyncQuery
            {
                OrderMenuItemIds = posCharge.PosChargeSelectedMenuItems!.Select(o => o.OrderMenuItemId),
                PageIndex = 0,
                PageSize = 1,
            });
            if (orderQuery.TotalItems == 0)
                throw new Exception($"PosCharge with Id {posCharge.Id} has no associated orders.");

            if (orderQuery.TotalItems > 1)
                throw new Exception($"PosCharge with Id {posCharge.Id} is trying to pay items of several orders.");

            var orderIds = orderQuery.Select(o => o.Id).ToList();
            var orderId = orderIds.Single();
            var jobId = backgroundJobHandler.Enqueue(() => posSyncService.ProcessOrders(orderIds, posCharge.MerchantId, OrderState.Draft, false));
        }

        public Task AssignPosChargeToSession(int id, int orderId) => commandProcessor.Execute(new UpdatePosChargesAsyncCommand
        {
            Criteria = new GetPosChargesCriteria
            {
                Ids = [id],
                PageSize = 1,
            },
            UpdateAction = async e =>
            {
                if (e.SessionId.HasValue)
                    return;

                var orderQuery = await queryProcessor.Execute(new GetOrdersAsyncQuery
                {
                    Ids = [orderId],
                    PageIndex = 0,
                    PageSize = 1,
                });
                var order = orderQuery.Single();
                e.SessionId = order.SessionId;
            }
        });

        public Task ExpireCharge(int chargeId) => commandProcessor.Execute(new UpdateChargesAsyncCommand
        {
            Criteria = new GetChargesCriteria
            {
                Ids = [chargeId],
                Statuses = [ChargeStatus.Requested, ChargeStatus.Processing],
                PageSize = 1,
            },
            UpdateAction = e =>
            {
                e.Status = ChargeStatus.Expired;
                return Task.CompletedTask;
            }
        });
    }
}