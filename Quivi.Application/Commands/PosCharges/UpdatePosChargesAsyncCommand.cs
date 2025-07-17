using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.PosCharges;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.PosPosCharges
{
    public interface IUpdatablePosCharge : IUpdatableEntity
    {
        int Id { get; }
        DateTime? CapturedDate { get; set; }
        int? SessionId { get; set; }
    }

    public class UpdatePosChargesAsyncCommand : AUpdateAsyncCommand<IEnumerable<PosCharge>, IUpdatablePosCharge>
    {
        public required GetPosChargesCriteria Criteria { get; init; }
    }

    public class UpdatePosChargesAsyncCommandHandler : ICommandHandler<UpdatePosChargesAsyncCommand, Task<IEnumerable<PosCharge>>>
    {
        private class UpdatablePosCharge : IUpdatablePosCharge
        {
            public readonly PosCharge Model;
            public DateTime? originalCapturedDate;
            public int? originalSessionId;

            public UpdatablePosCharge(PosCharge posCharge)
            {
                Model = posCharge;
                originalCapturedDate = posCharge.CaptureDate;
                originalSessionId = posCharge.SessionId;
            }

            public int Id => Model.Id;

            public DateTime? CapturedDate
            {
                get => Model.CaptureDate;
                set => Model.CaptureDate = value;
            }

            public int? SessionId
            {
                get => Model.SessionId;
                set => Model.SessionId = value;
            }

            public bool HasChanges
            {
                get
                {
                    if (originalCapturedDate != Model.CaptureDate)
                        return true;

                    if (originalSessionId != Model.SessionId)
                        return true;

                    return false;
                }
            }
        }

        private readonly IPosChargesRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public UpdatePosChargesAsyncCommandHandler(IPosChargesRepository repository,
                                                    IDateTimeProvider dateTimeProvider,
                                                    IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<IEnumerable<PosCharge>> Handle(UpdatePosChargesAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria);
            if (entities.Any() == false)
                return [];

            var now = dateTimeProvider.GetUtcNow();
            List<UpdatablePosCharge> changedEntities = new();
            foreach (var entity in entities)
            {
                var updatableEntity = new UpdatablePosCharge(entity);
                await command.UpdateAction(updatableEntity);
                if (updatableEntity.HasChanges)
                {
                    entity.ModifiedDate = now;
                    changedEntities.Add(updatableEntity);
                }
            }

            if(changedEntities.Any() == false)
                return entities;

            await repository.SaveChangesAsync();

            foreach (var entity in changedEntities)
                await eventService.Publish(new OnPosChargeOperationEvent
                {
                    Id = entity.Id,
                    ChannelId = entity.Model.ChannelId,
                    MerchantId = entity.Model.MerchantId,
                    Operation = EntityOperation.Update,
                });
            return entities;
        }
    }
}
