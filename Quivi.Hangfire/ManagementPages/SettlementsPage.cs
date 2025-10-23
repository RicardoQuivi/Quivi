using Hangfire;
using Hangfire.Dashboard.Management.v2.Metadata;
using Hangfire.Dashboard.Management.v2.Support;
using Hangfire.Server;
using Quivi.Application.Commands.Payouts;
using Quivi.Application.Commands.Settlements;
using Quivi.Application.Queries.PosCharges;
using Quivi.Infrastructure;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using System.ComponentModel;

namespace Quivi.Hangfire.ManagementPages
{
    [ManagementPage(MenuName = "Settlements", Title = "Settlements")]
    public class SettlementsPage : IJob
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IDateTimeProvider dateTimeProvider;

        public SettlementsPage(IQueryProcessor queryProcessor, ICommandProcessor commandProcessor, IDateTimeProvider dateTimeProvider)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.dateTimeProvider = dateTimeProvider;
        }

        [DisplayName("Process All")]
        [Description("Process all pending settlements")]
        public async Task ProcessSettlements(PerformContext context, IJobCancellationToken token)
        {
            var firstPosChargeQuery = await queryProcessor.Execute(new GetPosChargesAsyncQuery
            {
                QuiviPaymentsOnly = true,
                IsCaptured = true,

                PageIndex = 0,
                PageSize = 1,

                SortDirection = SortDirection.Ascending,
            });
            var firstPosCharge = firstPosChargeQuery.First();

            var now = dateTimeProvider.GetUtcNow();
            for (var date = firstPosCharge.CaptureDate!.Value.Add(-1 * QuiviConstants.SettlementOffset).Date; date <= now; date = date.AddDays(1))
            {
                await commandProcessor.Execute(new ProcessSettlementAsyncCommand
                {
                    Date = DateOnly.FromDateTime(date),
                });
            }
        }

        [DisplayName("Generate Payouts")]
        [Description("Safely generate unprocessed payouts")]
        public Task ProcessPayouts(PerformContext context, IJobCancellationToken token, [DisplayData("Settlement Id")] int settlementId)
        {
            return commandProcessor.Execute(new ProcessPayoutsAsyncCommand
            {
                SettlementId = settlementId,
            });
        }
    }
}