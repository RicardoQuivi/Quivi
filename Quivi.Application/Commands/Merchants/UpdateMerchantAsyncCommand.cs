using Quivi.Domain.Entities.Charges;
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
    public interface IUpdatableSurchargeFee : IUpdatableEntity
    {
        public ChargeMethod Method { get; }
        public decimal Fee { get; set; }
        public FeeUnit Unit { get; set; }
    }

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
        decimal SurchargeFee { get; set; }
        FeeUnit SurchargeFeeUnit { get; set; }
        bool IsDeleted { get; set; }
        bool IsDemo { get; set; }
        int? SetUpFeeId { get; set; }
        bool TermsAndConditionsAccepted { get; set; }
        IUpdatableRelationship<IUpdatableSurchargeFee, ChargeMethod> Surcharges { get; }
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

        private class UpdatableSurchargeFee : IUpdatableSurchargeFee
        {
            public readonly MerchantFee Model;
            private readonly bool isNew;
            private readonly decimal originalFee;
            private readonly FeeUnit originalFeeUnit;

            public UpdatableSurchargeFee(MerchantFee model, bool isNew)
            {
                this.Model = model;
                this.isNew = isNew;
                originalFee = model.Fee;
                originalFeeUnit = model.FeeUnit;
            }
            public ChargeMethod Method => Model.ChargeMethod;
            public decimal Fee { get => Model.Fee; set => Model.Fee = value; }
            public FeeUnit Unit { get => Model.FeeUnit; set => Model.FeeUnit = value; }
            public bool HasChanges
            {
                get
                {
                    if (isNew)
                        return true;

                    if (originalFee != Model.Fee)
                        return true;

                    if (originalFeeUnit != Model.FeeUnit)
                        return true;

                    return false;
                }
            }
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
            private readonly decimal? originalSurchargeFee;
            private readonly FeeUnit originalSurchargeFeeUnit;
            private readonly bool originalDeleted;
            private readonly int? originalSetUpFeeId;
            private readonly DateTime? originalTermsAndConditionsAcceptedDate;
            private readonly DateTime now;
            private readonly UpdatableRelationshipEntity<MerchantFee, IUpdatableSurchargeFee, ChargeMethod> surchargeFees;

            public UpdatableMerchant(Merchant merchant, DateTime now)
            {
                this.merchant = merchant;
                this.surchargeFees = new(merchant.SurchargeFees!.ToList(), fee => fee.ChargeMethod, t => new UpdatableSurchargeFee(t, t.CreatedDate == now), (method) =>
                {
                    var fee = new MerchantFee
                    {
                        Fee = merchant.SurchargeFee,
                        FeeUnit = merchant.SurchargeFeeUnit,
                        FeeType = FeeType.Surcharge,

                        MerchantId = merchant.Id,
                        Merchant = merchant,

                        ChargeMethod = method,

                        CreatedDate = now,
                        ModifiedDate = now,
                        DeletedDate = null,
                    };

                    //Workaround to only list surcharge fees. This is necessary because first argument of the constructor is not the "true" property
                    merchant.Fees!.Add(fee);
                    return fee;
                });

                this.now = now;

                originalParentMerchantId = merchant.ParentMerchantId;
                originalName = merchant.Name;
                originalIban = merchant.Iban;
                originalVatNumber = merchant.VatNumber;
                originalVatRate = merchant.VatRate;
                originalPostalCode = merchant.PostalCode;
                originalTransactionFee = merchant.TransactionFee;
                originalTransactionFeeUnit = merchant.TransactionFeeUnit;
                originalSurchargeFee = merchant.SurchargeFee;
                originalSurchargeFeeUnit = merchant.SurchargeFeeUnit;
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
            public decimal SurchargeFee
            {
                get => merchant.SurchargeFee;
                set => merchant.SurchargeFee = value;
            }
            public FeeUnit SurchargeFeeUnit
            {
                get => merchant.SurchargeFeeUnit;
                set => merchant.SurchargeFeeUnit = value;
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
            public IUpdatableRelationship<IUpdatableSurchargeFee, ChargeMethod> Surcharges => surchargeFees;

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

                    if (originalSurchargeFee != merchant.SurchargeFee)
                        return true;

                    if (originalSurchargeFeeUnit != merchant.SurchargeFeeUnit)
                        return true;

                    if (originalDeleted != merchant.DeletedDate.HasValue)
                        return true;

                    if (originalSetUpFeeId != merchant.SetUpFeeId)
                        return true;

                    if (originalTermsAndConditionsAcceptedDate != merchant.TermsAndConditionsAcceptedDate)
                        return true;

                    if (surchargeFees.HasChanges)
                        return true;

                    return false;
                }
            }
        }

        public async Task<IEnumerable<Merchant>> Handle(UpdateMerchantAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria with
            {
                IncludeFees = true,
            });
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
