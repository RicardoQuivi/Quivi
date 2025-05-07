using Quivi.Application.Queries.PosIntegrations;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events.Data.PosIntegrations;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Pos;

namespace Quivi.Hangfire.EventHandlers.PosIntegrations
{
    public class PosIntegrationOperationEventHandler : BackgroundEventHandler<OnPosIntegrationOperationEvent>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IPosSyncService posSyncService;

        public PosIntegrationOperationEventHandler(IQueryProcessor queryProcessor,
                                                    IPosSyncService posSyncService,
                                                    IBackgroundJobHandler backgroundJobHandler) : base(backgroundJobHandler)
        {
            this.queryProcessor = queryProcessor;
            this.posSyncService = posSyncService;
        }

        public override async Task Run(OnPosIntegrationOperationEvent message)
        {
            if (message.Operation != Infrastructure.Abstractions.Events.Data.EntityOperation.Create)
                return;

            var integrationsQuery = await queryProcessor.Execute(new GetPosIntegrationsAsyncQuery
            {
                Ids = [message.Id],
                PageSize = 1,
            });

            var integration = integrationsQuery.SingleOrDefault();
            if (integration?.DeletedDate.HasValue != false)
                return;

            await posSyncService.OnIntegrationSetUp(integration);
        }
    }
}
