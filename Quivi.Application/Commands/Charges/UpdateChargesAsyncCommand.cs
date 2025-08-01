using Quivi.Domain.Entities.Charges;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.PosCharges;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.Charges
{
    public interface IUpdatableCharge : IUpdatableEntity
    {
        int Id { get; }
        ChargeStatus Status { get; set; }
        ChargeMethod? Method { get; }
        ChargePartner? Partner { get; }
    }

    public class UpdateChargesAsyncCommand : AUpdateAsyncCommand<IEnumerable<Charge>, IUpdatableCharge>
    {
        public required GetChargesCriteria Criteria { get; init; }
    }

    public class UpdateChargesAsyncCommandHandler : ICommandHandler<UpdateChargesAsyncCommand, Task<IEnumerable<Charge>>>
    {
        private class UpdatableCharge : IUpdatableCharge
        {
            public readonly Charge Model;
            private readonly ChargeStatus originalStatus;

            public UpdatableCharge(Charge charge)
            {
                Model = charge;
                originalStatus = charge.Status;
            }

            public int Id => Model.Id;

            public ChargeStatus Status
            {
                get => Model.Status;
                set => Model.Status = value;
            }

            public ChargePartner? Partner => Model.ChargePartner;

            public ChargeMethod? Method => Model.ChargeMethod;

            public bool HasChanges
            {
                get
                {
                    if (originalStatus != Model.Status)
                        return true;

                    return false;
                }
            }
        }

        private readonly IChargesRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public UpdateChargesAsyncCommandHandler(IChargesRepository repository,
                                                IDateTimeProvider dateTimeProvider,
                                                IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<IEnumerable<Charge>> Handle(UpdateChargesAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria with
            {
                IncludePosCharge = true,
            });
            if (entities.Any() == false)
                return [];

            var now = dateTimeProvider.GetUtcNow();
            List<UpdatableCharge> changedEntities = new();
            foreach (var entity in entities)
            {
                var initialStatus = entity.Status;
                var updatableEntity = new UpdatableCharge(entity);
                await command.UpdateAction(updatableEntity);
                if (updatableEntity.HasChanges)
                {
                    entity.ModifiedDate = now;
                    changedEntities.Add(updatableEntity);
                }
            }

            if (changedEntities.Any() == false)
                return entities;

            await repository.SaveChangesAsync();
            foreach (var updatableCharge in changedEntities)
                await eventService.Publish(new OnPosChargeOperationEvent
                {
                    Id = updatableCharge.Id,
                    MerchantId = updatableCharge.Model.PosCharge!.MerchantId,
                    ChannelId = updatableCharge.Model.PosCharge!.ChannelId,
                    Operation = EntityOperation.Update,
                });
            return entities;
        }
    }
}
