using Hangfire;
using Quivi.Application.Commands.PreparationGroups;
using Quivi.Application.Queries.ChannelProfiles;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.PreparationGroups;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Hangfire.EventHandlers.PreparationGroups
{
    public class PreparationGroupOperationEventHandler : AddCommitedPreparationGroupAsyncCommandHandler, IEventHandler<OnPreparationGroupOperationEvent>
    {
        private readonly IQueryProcessor queryProcessor;

        public PreparationGroupOperationEventHandler(IQueryProcessor queryProcessor,
                                                        IPreparationGroupsRepository repository,
                                                        IDateTimeProvider dateTimeProvider,
                                                        IBackgroundJobHandler backgroundJobHandler,
                                                        IPosSyncService posSyncService,
                                                        IEventService eventService) : base(repository, dateTimeProvider, backgroundJobHandler, posSyncService, eventService)
        {
            this.queryProcessor = queryProcessor;
        }

        private string GetJobName(int preparationGroupId) => $"Commit-PreparationGroup-{preparationGroupId}";

        public Task Process(OnPreparationGroupOperationEvent message)
        {
            if (message.IsCommited == false)
                backgroundJobHandler.Enqueue(() => AutoProcess(message.MerchantId, message.Id));
            return Task.CompletedTask;
        }

        public async Task AutoProcess(int merchantId, int preparationGroupId)
        {
            var configurationsQuery = await queryProcessor.Execute(new GetChannelProfilesAsyncQuery
            {
                MerchantIds = [merchantId],
                PreparationGroupIds = [preparationGroupId],
                PageSize = 1,
            });

            var configuration = configurationsQuery.SingleOrDefault();
            if (configuration?.SendToPreparationTimer == null)
                return;

            var jobName = GetJobName(preparationGroupId);
            var command = new AddCommitedPreparationGroupAsyncCommand
            {
                MerchantId = merchantId,
                PreparationGroupId = preparationGroupId,
                IsPrepared = false,
            };
            RecurringJob.AddOrUpdate(jobName, () => Execute(command), configuration.SendToPreparationTimer.Value.ToCron());
        }

        public override async Task Execute(AddCommitedPreparationGroupAsyncCommand command)
        {
            await base.Execute(command);

            var jobName = GetJobName(command.PreparationGroupId);
            RecurringJob.RemoveIfExists(jobName);
        }
    }
}
