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
    public class UpsertMerchantAcquirerConfigurationAsyncCommand : AUpdateAsyncCommand<MerchantAcquirerConfiguration, IUpdatableMerchantAcquirerConfiguration>
    {
        public int MerchantId { get; set; }
        public ChargePartner ChargePartner { get; set; }
        public ChargeMethod ChargeMethod { get; set; }
    }

    internal class UpsertMerchantAcquirerConfigurationAsyncCommandHandler : ICommandHandler<UpsertMerchantAcquirerConfigurationAsyncCommand, Task<MerchantAcquirerConfiguration>>
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
