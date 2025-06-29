using Quivi.Domain.Entities.Notifications;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.PrinterNotificationTargets;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.PrinterMessageTargets
{
    public interface IUpdatablePrinterMessageTarget : IUpdatableEntity
    {
        int PrinterNotificationMessageId { get; }
        int PrinterNotificationsContactId { get; }
        DateTime CreatedDate { get; }
        DateTime? RequestedAt { get; set; }
        DateTime? FinishedAt { get; set; }
        AuditStatus Status { get; set; }
    }

    public class UpdatePrinterMessageTargetsAsyncCommand : AUpdateAsyncCommand<IEnumerable<PrinterMessageTarget>, IUpdatablePrinterMessageTarget>
    {
        public required GetPrinterMessageTargetsCriteria Criteria { get; init; }
    }

    public class UpdatePrinterMessageTargetsAsyncCommandHandler : ICommandHandler<UpdatePrinterMessageTargetsAsyncCommand, Task<IEnumerable<PrinterMessageTarget>>>
    {
        private readonly IPrinterMessageTargetsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public UpdatePrinterMessageTargetsAsyncCommandHandler(IPrinterMessageTargetsRepository repository,
                                                                IDateTimeProvider dateTimeProvider,
                                                                IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<IEnumerable<PrinterMessageTarget>> Handle(UpdatePrinterMessageTargetsAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria with
            {
                IncludePrinterNotificationMessage = true,
            });
            if (entities.Any() == false)
                return [];

            var now = dateTimeProvider.GetUtcNow();
            List<UpdatablePrinterMessageTarget> changedEntities = new();
            foreach (var entity in entities)
            {
                var updatableEntity = new UpdatablePrinterMessageTarget(entity);
                await command.UpdateAction.Invoke(updatableEntity);
                if (updatableEntity.HasChanges)
                {
                    entity.ModifiedDate = now;
                    changedEntities.Add(updatableEntity);
                }
            }

            if (changedEntities.Any() == false)
                return entities;

            await repository.SaveChangesAsync();

            foreach (var entity in changedEntities)
                await eventService.Publish(new OnPrinterMessageTargetOperationEvent
                {
                    PrinterNotificationMessageId = entity.PrinterNotificationMessageId,
                    PrinterNotificationsContactId = entity.PrinterNotificationsContactId,
                    MerchantId = entity.Model.PrinterNotificationMessage!.MerchantId,
                    Operation = EntityOperation.Update,
                });

            return entities;
        }

        private class UpdatablePrinterMessageTarget : IUpdatablePrinterMessageTarget
        {
            public PrinterMessageTarget Model { get; }
            private readonly DateTime? originalRequestedAt;
            private readonly DateTime? originalFinishedAt;
            private readonly AuditStatus originalStatus;

            public UpdatablePrinterMessageTarget(PrinterMessageTarget model)
            {
                this.Model = model;
                originalRequestedAt = this.Model.RequestedAt;
                originalFinishedAt = this.Model.FinishedAt;
                originalStatus = this.Model.Status;
            }

            public int PrinterNotificationMessageId => Model.PrinterNotificationMessageId;
            public int PrinterNotificationsContactId => Model.PrinterNotificationsContactId;
            public DateTime CreatedDate => Model.CreatedDate;

            public DateTime? RequestedAt
            { 
                get => Model.RequestedAt;
                set
                {
                    if (originalRequestedAt.HasValue)
                        throw new Exception($"Cannot change {nameof(RequestedAt)} because it is already commited");

                    Model.RequestedAt = value;
                }
            }
            public DateTime? FinishedAt
            {
                get => Model.FinishedAt;
                set
                {
                    if (originalFinishedAt.HasValue && value.HasValue == false)
                        throw new Exception($"Cannot change {nameof(FinishedAt)} from a value to a non value");

                    Model.FinishedAt = value;
                }
            }
            public AuditStatus Status
            { 
                get => Model.Status;
                set
                {
                    if (originalStatus != AuditStatus.Pending && value == AuditStatus.Pending)
                        throw new Exception($"Cannot change {nameof(Status)} to a pending value when it already concluded");

                    Model.Status = value;
                }
            }

            public bool HasChanges
            {
                get
                {
                    if (originalRequestedAt != Model.RequestedAt)
                        return true;

                    if (originalFinishedAt != Model.FinishedAt)
                        return true;

                    if (originalStatus != Model.Status)
                        return true;

                    return false;
                }
            }
        }
    }
}
