using Quivi.Domain.Entities.Financing;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.People
{
    public class UpsertSubmerchantFeesAccount
    {
        public required int ParentMerchantId { get; init; }
        public required int MerchantId { get; init; }
    }

    public class UpsertSubMerchantFeesAccountAsyncCommand : ICommand<Task<IEnumerable<Person>>>
    {
        public required IEnumerable<UpsertSubmerchantFeesAccount> SubmerchantFeesAccounts { get; init; }
    }

    public class UpsertSubMerchantFeesAccountAsyncCommandHandler : ICommandHandler<UpsertSubMerchantFeesAccountAsyncCommand, Task<IEnumerable<Person>>>
    {
        private readonly IPeopleRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;

        public UpsertSubMerchantFeesAccountAsyncCommandHandler(IPeopleRepository repository, IDateTimeProvider dateTimeProvider)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
        }

        public async Task<IEnumerable<Person>> Handle(UpsertSubMerchantFeesAccountAsyncCommand command)
        {
            var feesAccountsQuery = await repository.GetAsync(new GetPeopleCriteria
            {
                MerchantIds = command.SubmerchantFeesAccounts.Select(r => r.MerchantId),
                PersonTypes = [PersonType.Bill],
                HasMerchantService = false,
            });

            var feeAccountsDictionary = feesAccountsQuery.GroupBy(s => s.MerchantId!.Value).ToDictionary(r1 => r1.Key, r1 => r1.First());

            List<Person> result = new List<Person>();

            var now = dateTimeProvider.GetUtcNow();
            bool hasChanges = false;
            foreach (var r in command.SubmerchantFeesAccounts)
            {
                if (feeAccountsDictionary.TryGetValue(r.MerchantId, out var account))
                {
                    result.Add(account);
                    continue;
                }

                var person = new Person
                {
                    PersonType = PersonType.Bill,
                    PhoneNumber = null,
                    Vat = null,
                    ExpireDate = null,
                    IdentityNumber = null,

                    ParentMerchantId = r.ParentMerchantId,
                    MerchantId = r.MerchantId,

                    CreatedDate = now,
                    ModifiedDate = now,
                    DeletedDate = null,
                };
                repository.Add(person);
                result.Add(person);
                hasChanges = true;
            }

            if (hasChanges)
                await repository.SaveChangesAsync();

            return result;
        }
    }
}