using Quivi.Application.Commands.Charges;
using Quivi.Application.Commands.PosPosCharges;
using Quivi.Application.Extensions;
using Quivi.Application.Queries.MerchantAcquirerConfigurations;
using Quivi.Application.Queries.Orders;
using Quivi.Application.Queries.PosCharges;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Cqrs;
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

            if(posCharge.CaptureDate.HasValue)
            {
                await ProcessCaptured(posCharge);
                ProcessChainedCharge(posCharge);
                return;
            }

            await commandProcessor.Execute(new UpdateChargesAsyncCommand
            {
                Criteria = new GetChargesCriteria
                {
                    Ids = [posCharge.Charge!.Id],
                    Statuses = [ChargeStatus.Requested],
                    PageSize = 1,
                },
                UpdateAction = async e =>
                {
                    e.Status = ChargeStatus.Processing;
                    this.backgroundJobHandler.Schedule(() => ExpireCharge(message.Id), new DateTimeOffset(posCharge.CreatedDate.AddMinutes(10), TimeSpan.Zero));

                    if (e.Method == ChargeMethod.CreditCard && e.Partner.HasValue)
                        await AcquirePayment(posCharge.ChargeId, e.Method.Value, e.Partner.Value);
                }
            });
        }

        private void ProcessChainedCharge(PosCharge posCharge)
        {
            if(posCharge.Charge!.ChainedChargeId.HasValue == false)
                return;

            //TODO: Implement the logic to process the chained charge
            //await commandProcessor.Execute(new ProcessChainedChargeAsyncCommand
            //{
            //    ChargeId = posCharge.Charge.ChainedChargeId.Value,
            //});
        }

        private async Task ProcessCaptured(PosCharge posCharge)
        {
            if (posCharge.CaptureDate.HasValue == false)
                return;

            switch (posCharge.GetPosChargeType())
            {
                case PosChargeType.FreePayment: return;
                case PosChargeType.PayAtTheTable: ValidateAndSyncCharge(posCharge); break;
                case PosChargeType.OrderAndPay: await ValidateAndSyncOrder(posCharge); break;
            }
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
            //var assignmentJobId = backgroundJobHandler.ContinueJobWith(jobId, () => AssignPosChargeToSession(posCharge.Id, orderId));
            //backgroundJobHandler.ContinueJobWith(assignmentJobId, () => posSyncService.ProcessCharge(posCharge.Id));
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
                Statuses = [ChargeStatus.Requested],
                PageSize = 1,
            },
            UpdateAction = e =>
            {
                e.Status = ChargeStatus.Expired;
                return Task.CompletedTask;
            }
        });

        private async Task AcquirePayment(int channelId, ChargeMethod method, ChargePartner partner)
        {
            var configurationQuery = await queryProcessor.Execute(new GetMerchantAcquirerConfigurationsAsyncQuery
            {
                ChannelIds = [channelId],
                ChargeMethods = [method],
                ChargePartners = [partner],
                PageSize = 1,
                PageIndex = 0,
            });

            var configuration = configurationQuery.SingleOrDefault();
            if (configuration == null)
                return;

            //TODO: Implement the actual payment acquisition logic based on the configuration
            //if (configuration.ChargePartner == ChargePartner.SibsPaymentGateway)
            //    return await commandProcessor.Execute(new StartSpgCreditCardChargeAsyncCommand(charge));

            //if (configuration.ChargePartner == ChargePartner.Stripe)
            //    return await commandProcessor.Execute(new StartStripeCreditCardChargeAsyncCommand(charge));

            //if (configuration.ChargePartner == ChargePartner.Checkout)
            //    return await commandProcessor.Execute(new StartCheckoutCreditCardChargeAsyncCommand(charge));
        }
    }
}