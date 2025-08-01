using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Merchants;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.MerchantAcquirerConfigurations;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.MerchantAcquirerConfigurations
{
    public interface IUpdatableMerchantAcquirerConfiguration : IUpdatableEntity
    {
        int MerchantId { get; }
        ChargePartner ChargePartner { get; }
        ChargeMethod ChargeMethod { get; }

        public string? ApiKey { get; set; }
        public string? EntityId { get; set; }
        public string? TerminalId { get; set; }
        public string? WebhookSecret { get; set; }
        public string? PublicKey { get; set; }
        public bool ExternallySettled { get; set; }

        public bool Inactive { get; set; }
    }


    public class UpsertMerchantAcquirerConfigurationAsyncCommand : AUpdateAsyncCommand<MerchantAcquirerConfiguration, IUpdatableMerchantAcquirerConfiguration>
    {
        public int MerchantId { get; set; }
        public ChargePartner ChargePartner { get; set; }
        public ChargeMethod ChargeMethod { get; set; }
    }

    public class UpsertMerchantAcquirerConfigurationAsyncCommandHandler : ICommandHandler<UpsertMerchantAcquirerConfigurationAsyncCommand, Task<MerchantAcquirerConfiguration>>
    {
        private readonly IMerchantAcquirerConfigurationsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public UpsertMerchantAcquirerConfigurationAsyncCommandHandler(IMerchantAcquirerConfigurationsRepository repository,
                                                                        IDateTimeProvider dateTimeProvider,
                                                                        IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        private class UpdatableMerchantAcquirerConfiguration : IUpdatableMerchantAcquirerConfiguration
        {
            public readonly MerchantAcquirerConfiguration Model;
            private readonly DateTime now;
            private readonly string? originalApiKey;
            private readonly string? originalEntityId;
            private readonly string? originalTerminalId;
            private readonly string? originalWebhookSecret;
            private readonly string? originalPublicKey;
            private readonly bool originalExternallySettled;
            private readonly bool originalDeleted;

            public UpdatableMerchantAcquirerConfiguration(MerchantAcquirerConfiguration model, DateTime now)
            {
                this.Model = model;
                this.now = now;

                this.originalApiKey = model.ApiKey;
                this.originalEntityId = model.EntityId;
                this.originalTerminalId = model.TerminalId;
                this.originalWebhookSecret = model.WebhookSecret;
                this.originalPublicKey = model.PublicKey;
                this.originalExternallySettled = model.ExternallySettled;
                this.originalDeleted = model.DeletedDate.HasValue;
            }

            public int MerchantId => Model.MerchantId;
            public ChargePartner ChargePartner => Model.ChargePartner;
            public ChargeMethod ChargeMethod => Model.ChargeMethod;

            public string? ApiKey
            {
                get => Model.ApiKey;
                set => Model.ApiKey = value;
            }
            public string? EntityId
            {
                get => Model.EntityId;
                set => Model.EntityId = value;
            }
            public string? TerminalId
            {
                get => Model.TerminalId;
                set => Model.TerminalId = value;
            }
            public string? WebhookSecret
            {
                get => Model.WebhookSecret;
                set => Model.WebhookSecret = value;
            }
            public string? PublicKey
            {
                get => Model.PublicKey;
                set => Model.PublicKey = value;
            }
            public bool ExternallySettled
            {
                get => Model.ExternallySettled;
                set => Model.ExternallySettled = value;
            }
            public bool Inactive
            {
                get => Model.DeletedDate.HasValue;
                set => Model.DeletedDate = value ? this.now : null;
            }

            public bool WasDeleted => originalDeleted == false && Model.DeletedDate.HasValue;
            public bool WasRestored => originalDeleted && Model.DeletedDate.HasValue == false;

            public bool HasChanges
            {
                get
                {
                    if (ApiKey != originalApiKey)
                        return true;

                    if (EntityId != originalEntityId)
                        return true;

                    if (TerminalId != originalTerminalId)
                        return true;

                    if (WebhookSecret != originalWebhookSecret)
                        return true;

                    if (PublicKey != originalPublicKey)
                        return true;

                    if (ExternallySettled != originalExternallySettled)
                        return true;

                    if (originalDeleted != Model.DeletedDate.HasValue)
                        return true;

                    return false;
                }
            }
        }

        public async Task<MerchantAcquirerConfiguration> Handle(UpsertMerchantAcquirerConfigurationAsyncCommand command)
        {
            var entities = await repository.GetAsync(new GetMerchantAcquirerConfigurationsCriteria
            {
                MerchantIds = [command.MerchantId],
                ChargeMethods = [command.ChargeMethod],
                ChargePartners = [command.ChargePartner],
                PageIndex = 0,
                PageSize = 1,
            });
            var now = dateTimeProvider.GetUtcNow();
            var entity = entities.SingleOrDefault();
            bool hasChanges = false;

            if (entity == null)
            {
                entity = new MerchantAcquirerConfiguration
                {
                    MerchantId = command.MerchantId,
                    ChargePartner = command.ChargePartner,
                    ChargeMethod = command.ChargeMethod,
                    CreatedDate = now,
                    ModifiedDate = now,
                };
                repository.Add(entity);
                hasChanges = true;
            }
            var updatableEntity = new UpdatableMerchantAcquirerConfiguration(entity, now);
            updatableEntity.Inactive = false;
            await command.UpdateAction.Invoke(updatableEntity);

            if (updatableEntity.HasChanges)
            {
                updatableEntity.Model.ModifiedDate = now;
                hasChanges = true;
            }

            if (hasChanges == false)
                return updatableEntity.Model;

            await repository.SaveChangesAsync();
            await eventService.Publish(new OnMerchantAcquirerConfigurationEvent
            {
                Id = updatableEntity.Model.Id,
                MerchantId = updatableEntity.Model.MerchantId,
                Operation = updatableEntity.WasDeleted ? EntityOperation.Delete : (entity.CreatedDate == now || updatableEntity.WasRestored ? EntityOperation.Create : EntityOperation.Update),
            });

            return updatableEntity.Model;
        }
    }
}
