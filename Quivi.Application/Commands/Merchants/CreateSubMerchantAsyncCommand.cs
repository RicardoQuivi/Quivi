using Quivi.Domain.Entities.Financing;
using Quivi.Domain.Entities.Identity;
using Quivi.Domain.Entities.Merchants;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.Merchants;
using Quivi.Infrastructure.Abstractions.Repositories;

namespace Quivi.Application.Commands.Merchants
{
    public class CreateSubMerchantAsyncCommand : ICommand<Task<Merchant>>
    {
        public required int ParentId { get; init; }
        public required string LogoUrl { get; init; }
        public string? FiscalName { get; init; }
        public required string Name { get; init; }
        public decimal VatRate { get; init; } = 23;
        public decimal TransactionFee { get; init; } = 1.5m;
        public string? Iban { get; init; }
        public string? IbanProofUrl { get; init; }
        public string? StreetAddress { get; init; }
        public string? City { get; init; }
        public required string PostalCode { get; init; }
    }

    public class CreateSubMerchantAsyncCommandHandler : ICommandHandler<CreateSubMerchantAsyncCommand, Task<Merchant>>
    {
        private readonly IMerchantsRepository repo;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;
        private readonly IRandomGenerator randomGenerator;

        public CreateSubMerchantAsyncCommandHandler(IMerchantsRepository repository,
                                                    IDateTimeProvider dateTimeProvider,
                                                    IEventService eventService,
                                                    IRandomGenerator randomGenerator)
        {
            this.repo = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
            this.randomGenerator = randomGenerator;
        }

        public async Task<Merchant> Handle(CreateSubMerchantAsyncCommand command)
        {
            var now = dateTimeProvider.GetUtcNow();
            Merchant merchant = AddSubMerchant(command, now);
            await repo.SaveChangesAsync();

            await eventService.Publish(new OnMerchantOperationEvent
            {
                Id = merchant.Id,
                Operation = EntityOperation.Create,
                ParentId = merchant.ParentMerchantId,
            });

            return merchant;
        }

        private Merchant AddSubMerchant(CreateSubMerchantAsyncCommand command, DateTime now)
        {
            var merchant = new Merchant
            {
                FiscalName = command.FiscalName,
                Name = command.Name,
                LogoUrl = command.LogoUrl,
                IbanProofUrl = command.IbanProofUrl,
                CreatedDate = now,
                ModifiedDate = now,
            };

            merchant.ParentMerchantId = command.ParentId;
            merchant.TransactionFee = command.TransactionFee;
            merchant.TransactionFeeUnit = FeeUnit.Percentage;
            merchant.VatRate = command.VatRate;
            merchant.Iban = command.Iban;
            merchant.City = command.City;
            merchant.StreetAddress = command.StreetAddress;
            merchant.PostalCode = command.PostalCode;
            merchant.TimeZone = GetTimeZoneByPostalCode(command.PostalCode);
            merchant.ApiClients = new List<ApiClient>
            {
                new ApiClient
                {
                    UserName = randomGenerator.String(8),
                    Password = randomGenerator.String(32),
                    ClientType = BasicAuthClientType.Backoffice,
                    CreatedDate = now,
                    Person = new Person
                    {
                        CreatedDate = now,
                        PersonType = PersonType.Channel,
                        SubMerchant = merchant,
                        MerchantId = merchant.ParentMerchantId,
                    },
                },
                new ApiClient
                {
                    UserName = randomGenerator.String(8),
                    Password = randomGenerator.String(32),
                    ClientType = BasicAuthClientType.GuestsApp,
                    CreatedDate = now,
                    Person = new Person
                    {
                        CreatedDate = now,
                        PersonType = PersonType.Channel,
                        SubMerchant = merchant,
                        MerchantId = merchant.ParentMerchantId,
                    },
                },
            };

            SetUpFees(merchant);

            repo.Add(merchant);
            return merchant;
        }

        private static void SetUpFees(Merchant merchant)
        {
            merchant.Fees = [];
        }

        private static string GetTimeZoneByPostalCode(string postalCode)
        {
            postalCode = postalCode.Replace("-", "").Trim();

            if (postalCode.Length > 4)
                postalCode = postalCode.Substring(0, 4);

            if (int.TryParse(postalCode, out int convertedPostalCode))
            {
                if (convertedPostalCode >= 9500 && convertedPostalCode <= 9995)
                    return "Azores Standard Time";
            }

            return "GMT Standard Time";
        }
    }
}
