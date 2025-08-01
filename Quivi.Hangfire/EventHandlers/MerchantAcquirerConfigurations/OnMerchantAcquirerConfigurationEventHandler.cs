using Quivi.Application.Queries.MerchantAcquirerConfigurations;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.MerchantAcquirerConfigurations;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Services.Charges;

namespace Quivi.Hangfire.EventHandlers.MerchantAcquirerConfigurations
{
    public class OnMerchantAcquirerConfigurationEventHandler : BackgroundEventHandler<OnMerchantAcquirerConfigurationEvent>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IEnumerable<IAcquirerProcessingStrategy> acquirerStrategies;

        public OnMerchantAcquirerConfigurationEventHandler(IEnumerable<IAcquirerProcessingStrategy> acquirerStrategies,
                                                            IQueryProcessor queryProcessor,
                                                            IBackgroundJobHandler backgroundJobHandler) : base(backgroundJobHandler)
        {
            this.queryProcessor = queryProcessor;
            this.acquirerStrategies = acquirerStrategies;
        }

        public override Task Run(OnMerchantAcquirerConfigurationEvent message)
        {
            if (message.Operation == EntityOperation.Create)
                return ProcessSetUp(message);

            if (message.Operation == EntityOperation.Delete)
                return ProcessTearDown(message);

            return Task.CompletedTask;
        }

        private async Task ProcessSetUp(OnMerchantAcquirerConfigurationEvent message)
        {
            var acquirer = await GetAcquirer(message);
            if (acquirer == null || acquirer.DeletedDate.HasValue)
                return;

            var strategy = acquirerStrategies.FirstOrDefault(c => c.ChargePartner == acquirer.ChargePartner && c.ChargeMethod == acquirer.ChargeMethod);
            if (strategy == null)
                return;

            await strategy.OnSetup(acquirer);
        }

        private async Task<Domain.Entities.Merchants.MerchantAcquirerConfiguration?> GetAcquirer(OnMerchantAcquirerConfigurationEvent message)
        {
            var acquirerQuery = await queryProcessor.Execute(new GetMerchantAcquirerConfigurationsAsyncQuery
            {
                Ids = [message.Id],
                MerchantIds = [message.MerchantId],
                PageSize = 1,
            });
            var acquirer = acquirerQuery.SingleOrDefault();
            return acquirer;
        }

        private async Task ProcessTearDown(OnMerchantAcquirerConfigurationEvent message)
        {
            var acquirer = await GetAcquirer(message);
            if (acquirer == null)
                return;

            var strategy = acquirerStrategies.FirstOrDefault(c => c.ChargePartner == acquirer.ChargePartner && c.ChargeMethod == acquirer.ChargeMethod);
            if (strategy == null)
                return;

            await strategy.OnTearDown(acquirer);
        }
    }
}