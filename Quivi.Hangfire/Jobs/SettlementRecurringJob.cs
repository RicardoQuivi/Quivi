using Hangfire;
using Quivi.Application.Commands.Settlements;
using Quivi.Hangfire.Hangfire;
using Quivi.Infrastructure;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;

namespace Quivi.Hangfire.Jobs
{
    public class SettlementRecurringJob : IRecurringJob
    {
        public string Schedule
        {
            get
            {
                var offset = QuiviConstants.SettlementOffset;
                return Cron.Daily(offset.Hours, offset.Minutes);
            }
        }

        private readonly ICommandProcessor commandProcessor;
        private readonly IDateTimeProvider dateTimeProvider;

        public SettlementRecurringJob(ICommandProcessor commandProcessor, IDateTimeProvider dateTimeProvider)
        {
            this.commandProcessor = commandProcessor;
            this.dateTimeProvider = dateTimeProvider;
        }

        public Task Run() => commandProcessor.Execute(new ProcessSettlementAsyncCommand
        {
            Date = DateOnly.FromDateTime(dateTimeProvider.GetUtcNow().AddDays(-1)),
        });
    }
}