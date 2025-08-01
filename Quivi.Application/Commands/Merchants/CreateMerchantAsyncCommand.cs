using Quivi.Application.Commands.CustomChargeMethods;
using Quivi.Application.Commands.MerchantAcquirerConfigurations;
using Quivi.Application.Commands.PosIntegrations;
using Quivi.Application.Configurations;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Merchants;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Configurations;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.Merchants;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Extensions;
using Quivi.Infrastructure.Validations;

namespace Quivi.Application.Commands.Merchants
{
    public class CreateMerchantAsyncCommand : ICommand<Task<Merchant?>>
    {
        public required int UserId { get; init; }
        public required string VatNumber { get; init; }
        public required string Name { get; init; }
        public required string FiscalName { get; init; }
        public required string PostalCode { get; init; }
        public required string Iban { get; init; }
        public required string IbanProofUrl { get; init; }
        public required string LogoUrl { get; init; }
        public decimal VatRate { get; init; } = 23;
        public decimal TransactionFee { get; init; } = 1.5m;
        public required Action OnInvalidMerchantName { get; init; }
        public required Action OnInvalidVatNumber { get; init; }
        public required Action OnInvalidIban { get; init; }
        public required Action OnVatNumberAlreadyExists { get; init; }
    }

    public class CreateMerchantAsyncCommandHandler : ICommandHandler<CreateMerchantAsyncCommand, Task<Merchant?>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IAppHostsSettings appHostsSettings;
        private readonly ICommandProcessor commandProcessor;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;
        private readonly IPaybyrdSettings paybyrdSettings;

        public CreateMerchantAsyncCommandHandler(IUnitOfWork unitOfWork,
                                                    IAppHostsSettings appHostsSettings,
                                                    ICommandProcessor commandProcessor,
                                                    IDateTimeProvider dateTimeProvider,
                                                    IEventService eventService,
                                                    IPaybyrdSettings paybyrdSettings)
        {
            this.unitOfWork = unitOfWork;
            this.appHostsSettings = appHostsSettings;
            this.dateTimeProvider = dateTimeProvider;
            this.commandProcessor = commandProcessor;
            this.eventService = eventService;
            this.paybyrdSettings = paybyrdSettings;
        }

        public async Task<Merchant?> Handle(CreateMerchantAsyncCommand command)
        {
            bool hasErrors = false;

            if (string.IsNullOrWhiteSpace(command.Name))
            {
                command.OnInvalidMerchantName();
                hasErrors = true;
            }

            if (string.IsNullOrWhiteSpace(command.Iban) || command.Iban.IsValidIban() == false)
            {
                command.OnInvalidIban();
                hasErrors = true;
            }

            if (string.IsNullOrWhiteSpace(command.VatNumber) || command.VatNumber.IsValidNif() == false)
            {
                command.OnInvalidVatNumber();
                hasErrors = true;
                return null;
            }

            var merchantRepo = unitOfWork.Merchants;
            var exists = await VatNumberExists(command, merchantRepo);
            if (exists)
            {
                command.OnVatNumberAlreadyExists();
                hasErrors = true;
            }

            if (hasErrors)
                return null;

            await using var transaction = await unitOfWork.StartTransactionAsync();
            try
            {
                Merchant merchant = await AddMerchant(command, merchantRepo);
                Merchant subMerchant = await commandProcessor.Execute(new CreateSubMerchantAsyncCommand
                {
                    ParentId = merchant.Id,
                    FiscalName = command.FiscalName,
                    Name = command.Name,
                    LogoUrl = command.LogoUrl,
                    PostalCode = command.PostalCode,
                    VatRate = command.VatRate,
                    TransactionFee = command.TransactionFee,
                    Iban = command.Iban,
                });
                await commandProcessor.Execute(new AddPosIntegrationAsyncCommand
                {
                    MerchantId = subMerchant.Id,
                    ConnectionString = Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        SkipInvoice = "1",
                        AccessToken = "",
                        IncludeTipInInvoice = "0",
                    }),
                    DiagnosticErrorsMuted = false,
                    IntegrationType = IntegrationType.QuiviViaFacturalusa,
                });
                await CreateCustomChargeMethods(subMerchant);
                await CreateAcquirerConfigurations(subMerchant);

                await commandProcessor.Execute(new AddUserToMerchantsAsyncCommand
                {
                    UserId = command.UserId,
                    MerchantIds = [merchant.Id, subMerchant.Id],
                });

                await transaction.CommitAsync();
                return subMerchant;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task CreateCustomChargeMethods(Merchant subMerchant)
        {
            await commandProcessor.Execute(new AddCustomChargeMethodAsyncCommand
            {
                MerchantId = subMerchant.Id,
                Name = "Dinheiro",
                LogoUrl = appHostsSettings.BackofficeApi.CombineUrl("/Images/chargemethods/cash.svg"),
                OnInvalidName = () => throw new Exception("New Merchant. This cannot happen."),
                OnNameAlreadyExists = () => throw new Exception("New Merchant. This cannot happen."),
            });
            await commandProcessor.Execute(new AddCustomChargeMethodAsyncCommand
            {
                MerchantId = subMerchant.Id,
                Name = "Cartão Refeição Ticket",
                LogoUrl = appHostsSettings.BackofficeApi.CombineUrl("/Images/chargemethods/ticket-restaurant.svg"),
                OnInvalidName = () => throw new Exception("New Merchant. This cannot happen."),
                OnNameAlreadyExists = () => throw new Exception("New Merchant. This cannot happen."),
            });
            await commandProcessor.Execute(new AddCustomChargeMethodAsyncCommand
            {
                MerchantId = subMerchant.Id,
                Name = "MB Way",
                LogoUrl = appHostsSettings.BackofficeApi.CombineUrl("/Images/chargemethods/mb-way.svg"),
                OnInvalidName = () => throw new Exception("New Merchant. This cannot happen."),
                OnNameAlreadyExists = () => throw new Exception("New Merchant. This cannot happen."),
            });
            await commandProcessor.Execute(new AddCustomChargeMethodAsyncCommand
            {
                MerchantId = subMerchant.Id,
                Name = "Cartão de Crédito/Débito",
                LogoUrl = appHostsSettings.BackofficeApi.CombineUrl("/Images/chargemethods/credit-card.svg"),
                OnInvalidName = () => throw new Exception("New Merchant. This cannot happen."),
                OnNameAlreadyExists = () => throw new Exception("New Merchant. This cannot happen."),
            });
        }

        private async Task CreateAcquirerConfigurations(Merchant subMerchant)
        {
            await commandProcessor.Execute(new UpsertMerchantAcquirerConfigurationAsyncCommand
            {
                MerchantId = subMerchant.Id,
                ChargeMethod = ChargeMethod.MbWay,
                ChargePartner = ChargePartner.Paybyrd,
                UpdateAction = r =>
                {
                    r.ApiKey = paybyrdSettings.ApiKey;
                    r.Inactive = false;
                    return Task.CompletedTask;
                }
            });

            await commandProcessor.Execute(new UpsertMerchantAcquirerConfigurationAsyncCommand
            {
                MerchantId = subMerchant.Id,
                ChargeMethod = ChargeMethod.CreditCard,
                ChargePartner = ChargePartner.Paybyrd,
                UpdateAction = r =>
                {
                    r.ApiKey = paybyrdSettings.ApiKey;
                    r.Inactive = false;
                    return Task.CompletedTask;
                }
            });
        }

        private async Task<Merchant> AddMerchant(CreateMerchantAsyncCommand command, IMerchantsRepository merchantRepo)
        {
            var now = dateTimeProvider.GetUtcNow();
            var merchant = new Merchant
            {
                FiscalName = command.FiscalName,
                Name = command.Name,
                VatNumber = command.VatNumber,
                LogoUrl = command.LogoUrl,
                IbanProofUrl = command.IbanProofUrl,
                CreatedDate = now,
                ModifiedDate = now,
            };
            merchantRepo.Add(merchant);
            await merchantRepo.SaveChangesAsync();

            await eventService.Publish(new OnMerchantOperationEvent
            {
                Id = merchant.Id,
                Operation = EntityOperation.Create,
                ParentId = null,
            });
            return merchant;
        }

        private static async Task<bool> VatNumberExists(CreateMerchantAsyncCommand command, IMerchantsRepository merchantRepo)
        {
            var vatNumberQuery = await merchantRepo.GetAsync(new Infrastructure.Abstractions.Repositories.Criterias.GetMerchantsCriteria
            {
                VatNumbers = [command.VatNumber],
                PageSize = 0,
            });
            return vatNumberQuery.TotalItems > 0;
        }
    }
}