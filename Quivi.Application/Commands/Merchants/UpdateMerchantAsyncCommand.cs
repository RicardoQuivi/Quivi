using Quivi.Domain.Entities.Merchants;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.Merchants;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.Merchants
{
    public interface IUpdatableMerchant : IUpdatableEntity
    {
        int Id { get; }
        int? ParentMerchantId { get; }
        string Name { get; set; }
        string? Iban { get; set; }
        string? VatNumber { get; set; }
        decimal? VatRate { get; set; }
        string? PostalCode { get; set; }
        string? LogoUrl { get; set; }
        decimal TransactionFee { get; set; }
        FeeUnit TransactionFeeUnit { get; set; }
        bool IsDeleted { get; set; }
        bool IsDemo { get; set; }
        int? SetUpFeeId { get; set; }
        bool TermsAndConditionsAccepted { get; set; }
    }

    public class UpdateMerchantAsyncCommand : AUpdateAsyncCommand<IEnumerable<Merchant>, IUpdatableMerchant>
    {
        public required GetMerchantsCriteria Criteria { get; init; }
    }

    public class UpdateMerchantAsyncCommandHandler : ICommandHandler<UpdateMerchantAsyncCommand, Task<IEnumerable<Merchant>>>
    {
        private readonly IMerchantsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public UpdateMerchantAsyncCommandHandler(IMerchantsRepository repository, IDateTimeProvider dateTimeProvider, IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        private class UpdatableMerchant : IUpdatableMerchant
        {
            private readonly Merchant merchant;
            private readonly int? originalParentMerchantId;
            private readonly string originalName;
            private readonly string? originalIban;
            private readonly string? originalVatNumber;
            private readonly decimal? originalVatRate;
            private readonly string? originalPostalCode;
            private readonly string? originalLogoUrl;
            private readonly decimal? originalTransactionFee;
            private readonly FeeUnit originalTransactionFeeUnit;
            private readonly bool originalDeleted;
            private readonly int? originalSetUpFeeId;
            private readonly DateTime? originalTermsAndConditionsAcceptedDate;
            private readonly DateTime now;

            public UpdatableMerchant(Merchant merchant, DateTime now)
            {
                this.merchant = merchant;
                this.now = now;

                originalParentMerchantId = merchant.ParentMerchantId;
                originalName = merchant.Name;
                originalIban = merchant.Iban;
                originalVatNumber = merchant.VatNumber;
                originalVatRate = merchant.VatRate;
                originalPostalCode = merchant.PostalCode;
                originalTransactionFee = merchant.TransactionFee;
                originalTransactionFeeUnit = merchant.TransactionFeeUnit;
                originalDeleted = merchant.DeletedDate.HasValue;
                originalSetUpFeeId = merchant.SetUpFeeId;
                originalTermsAndConditionsAcceptedDate = merchant.TermsAndConditionsAcceptedDate;
            }

            public int Id => merchant.Id;
            public int? ParentMerchantId => merchant.ParentMerchantId;
            public string Name
            {
                get => merchant.Name;
                set => merchant.Name = value;
            }
            public string? Iban
            {
                get => merchant.Iban;
                set => merchant.Iban = value;
            }
            public string? VatNumber
            {
                get => merchant.VatNumber;
                set => merchant.VatNumber = value;
            }
            public decimal? VatRate
            {
                get => merchant.VatRate;
                set => merchant.VatRate = value;
            }
            public string? PostalCode
            {
                get => merchant.PostalCode;
                set => merchant.PostalCode = value;
            }
            public string? LogoUrl
            {
                get => merchant.LogoUrl;
                set => merchant.LogoUrl = value;
            }
            public decimal TransactionFee
            {
                get => merchant.TransactionFee;
                set => merchant.TransactionFee = value;
            }
            public FeeUnit TransactionFeeUnit
            {
                get => merchant.TransactionFeeUnit;
                set => merchant.TransactionFeeUnit = value;
            }
            public bool IsDeleted
            {
                get => merchant.DeletedDate.HasValue;
                set => merchant.DeletedDate = value ? now : null;
            }
            public bool IsDemo
            {
                get => merchant.IsDemo;
                set => merchant.IsDemo = value;
            }
            public int? SetUpFeeId
            {
                get => merchant.SetUpFeeId;
                set => merchant.SetUpFeeId = value;
            }
            public bool TermsAndConditionsAccepted
            {
                get => merchant.TermsAndConditionsAcceptedDate.HasValue;
                set => merchant.TermsAndConditionsAcceptedDate = value ? now : null;
            }

            public bool WasDeleted => originalDeleted == false && IsDeleted;
            public bool HasChanges
            {
                get
                {
                    if (originalParentMerchantId != merchant.ParentMerchantId)
                        return true;

                    if (originalName != merchant.Name)
                        return true;

                    if (originalIban != merchant.Iban)
                        return true;

                    if (originalVatNumber != merchant.VatNumber)
                        return true;

                    if (originalVatRate != merchant.VatRate)
                        return true;

                    if (originalPostalCode != merchant.PostalCode)
                        return true;

                    if (originalLogoUrl != merchant.LogoUrl)
                        return true;

                    if (originalTransactionFee != merchant.TransactionFee)
                        return true;

                    if (originalTransactionFeeUnit != merchant.TransactionFeeUnit)
                        return true;

                    if (originalDeleted != merchant.DeletedDate.HasValue)
                        return true;

                    if (originalSetUpFeeId != merchant.SetUpFeeId)
                        return true;

                    if (originalTermsAndConditionsAcceptedDate != merchant.TermsAndConditionsAcceptedDate)
                        return true;

                    return false;
                }
            }
        }

        public async Task<IEnumerable<Merchant>> Handle(UpdateMerchantAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria);
            if (entities.Any() == false)
                return entities;

            var now = dateTimeProvider.GetUtcNow();
            var changedEntities = new List<UpdatableMerchant>();
            foreach (var entity in entities)
            {
                var updatableEntity = new UpdatableMerchant(entity, now);
                await command.UpdateAction.Invoke(updatableEntity);

                if (updatableEntity.HasChanges)
                    changedEntities.Add(updatableEntity);
            }

            if (changedEntities.Any() == false)
                return entities;

            await repository.SaveChangesAsync();

            foreach (var entity in changedEntities)
                await eventService.Publish(new OnMerchantOperationEvent
                {
                    Id = entity.Id,
                    Operation = entity.WasDeleted ? EntityOperation.Delete : EntityOperation.Update,
                });
            return entities;
        }
    }
}
