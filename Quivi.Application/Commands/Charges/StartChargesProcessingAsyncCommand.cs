using Quivi.Domain.Entities.Charges;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Services.Charges;

namespace Quivi.Application.Commands.Charges
{
    public class StartChargesProcessingAsyncCommand : ICommand<Task<IEnumerable<Charge>>>
    {
        public required GetChargesCriteria Criteria { get; init; }
    }

    public class StartChargesProcessingAsyncCommandHandler : ICommandHandler<StartChargesProcessingAsyncCommand, Task<IEnumerable<Charge>>>
    {
        private readonly IChargesRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEnumerable<IChargeProcessingStrategy> chargeProcessingStrategies;

        public StartChargesProcessingAsyncCommandHandler(IChargesRepository repository,
                                                    IEnumerable<IChargeProcessingStrategy> chargeProcessingStrategies,
                                                    IDateTimeProvider dateTimeProvider)
        {
            this.repository = repository;
            this.chargeProcessingStrategies = chargeProcessingStrategies;
            this.dateTimeProvider = dateTimeProvider;
        }
        public async Task<IEnumerable<Charge>> Handle(StartChargesProcessingAsyncCommand command)
        {
            var chargesQuery = await repository.GetAsync(command.Criteria);

            var now = dateTimeProvider.GetUtcNow();
            List<Charge> result = [];
            foreach (var charge in chargesQuery)
            {
                if (charge.Status != ChargeStatus.Requested)
                {
                    result.Add(charge);
                    continue;
                }
                var strategy = chargeProcessingStrategies.FirstOrDefault(c => c.ChargePartner == charge.ChargePartner && c.ChargeMethod == charge.ChargeMethod);
                if (strategy == null)
                    continue;

                try
                {
                    await strategy.ProcessChargeStatus(charge);
                    result.Add(charge);
                    charge.Status = ChargeStatus.Processing;
                    charge.ModifiedDate = now;
                }
                catch
                {
                    throw;
                }
            }

            return result;
        }
    }
}