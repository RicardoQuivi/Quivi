using Quivi.Application.Queries.Merchants;
using Quivi.Application.Queries.SettlementDetails;
using Quivi.Application.Queries.SettlementServiceDetails;
using Quivi.Domain.Entities.Financing;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Payouts;

namespace Quivi.Application.Commands.Payouts
{
    public class ProcessPayoutsAsyncCommand : ICommand<Task>
    {
        public int SettlementId { get; init; }
    }

    internal class ProcessPayoutsAsyncCommandHandler : ICommandHandler<ProcessPayoutsAsyncCommand, Task>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IPayoutProcessor payoutProcessor;

        public ProcessPayoutsAsyncCommandHandler(IPayoutProcessor payoutProcessor, IQueryProcessor queryProcessor)
        {
            this.payoutProcessor = payoutProcessor;
            this.queryProcessor = queryProcessor;
        }

        public async Task Handle(ProcessPayoutsAsyncCommand command)
        {
            var settlementDetails = await queryProcessor.Execute(new GetSettlementDetailsAsyncQuery
            {
                SettlementIds = [command.SettlementId],
                SettlementStates = [SettlementState.Finished],
                IsMerchantDemo = false,

                PageIndex = 0,
                PageSize = null,
            });

            var settlementServiceDetails = await queryProcessor.Execute(new GetSettlementServiceDetailsAsyncQuery
            {
                SettlementIds = [command.SettlementId],
                SettlementStates = [SettlementState.Finished],
                IsMerchantDemo = false,

                PageIndex = 0,
                PageSize = null,
            });

            var payoutRows = await GetPayoutRows(settlementDetails, settlementServiceDetails);
            if (payoutRows.Any() == false)
                return;

            await payoutProcessor.Process(payoutRows);
        }

        private async Task<IEnumerable<Payout>> GetPayoutRows(IEnumerable<SettlementDetail> details, IEnumerable<SettlementServiceDetail> serviceDetails)
        {
            var payouts = new List<Payout>();
            var merchantsQuery = await queryProcessor.Execute(new GetMerchantsAsyncQuery
            {
                Ids = details.Select(s => s.MerchantId).Concat(serviceDetails.Select(s => s.MerchantId)).Distinct(),

                PageIndex = 0,
                PageSize = null,
            });
            var merchantsDictionary = merchantsQuery.ToDictionary(r1 => r1.Id, r1 => r1.Name);

            var serviceDetailsDict = serviceDetails.GroupBy(d => d.MerchantId).ToDictionary(d => d.Key, d => d.ToList());
            foreach (var d in details.GroupBy(d => d.MerchantIban))
            {
                var first = d.First();
                var iban = first.MerchantIban;
                var settlementId = first.SettlementId;
                List<SettlementServiceDetail> s = new List<SettlementServiceDetail>();

                var merchantIds = d.Select(d1 => d1.MerchantId).Distinct();
                foreach (var merchantId in merchantIds)
                {
                    if (serviceDetailsDict.TryGetValue(merchantId, out var aux) == false)
                        continue;
                    s.AddRange(aux);
                }

                var amount = d.Sum(d1 => d1.NetAmount) + s.Sum(d1 => d1.TotalAmount);
                if (amount <= 0)
                    continue;

                string name = string.Join(" - ", merchantIds.Select(i => merchantsDictionary[i]));
                payouts.Add(new Payout
                {
                    Iban = iban.Replace(" ", ""),
                    Name = name,
                    MerchantId = first.MerchantId,
                    TransferReference = $"{settlementId}",
                    TransferAmount = amount,
                });
            }

            return payouts;
        }
    }
}
