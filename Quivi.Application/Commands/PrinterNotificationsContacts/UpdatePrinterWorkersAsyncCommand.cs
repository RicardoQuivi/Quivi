using Quivi.Domain.Entities.Notifications;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.PrinterNotificationsContacts;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.PrinterNotificationsContacts
{
    public interface IUpdatablePrinterNotificationsContact : IUpdatableEntity
    {
        int Id { get; }
        string Name { get; set; }
        string Address { get; set; }
        int PrinterWorkerId { get; set; }
        int? LocationId { get; set; }
        NotificationMessageType Notifications { get; set; }
        bool IsDeleted { get; set; }
    }

    public class UpdatePrinterNotificationsContactsAsyncCommand : AUpdateAsyncCommand<IEnumerable<PrinterNotificationsContact>, IUpdatablePrinterNotificationsContact>
    {
        public required GetPrinterNotificationsContactsCriteria Criteria { get; init; }
    }

    public class UpdatePrinterNotificationsContactsAsyncCommandHandler : ICommandHandler<UpdatePrinterNotificationsContactsAsyncCommand, Task<IEnumerable<PrinterNotificationsContact>>>
    {
        private readonly IPrinterNotificationsContactsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public UpdatePrinterNotificationsContactsAsyncCommandHandler(IPrinterNotificationsContactsRepository repository,
                                                IDateTimeProvider dateTimeProvider,
                                                IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<IEnumerable<PrinterNotificationsContact>> Handle(UpdatePrinterNotificationsContactsAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria with
            {
                IncludeNotificationsContact = true,
            });
            if (entities.Any() == false)
                return [];

            var now = dateTimeProvider.GetUtcNow();
            List<UpdatablePrinterNotificationsContact> changedEntities = new();
            foreach (var entity in entities)
            {
                var updatableEntity = new UpdatablePrinterNotificationsContact(entity, now);
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
                await eventService.Publish(new OnPrinterNotificationsContactOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = entity.Model.BaseNotificationsContact!.MerchantId,
                    Operation = entity.WasDeleted ? EntityOperation.Delete : EntityOperation.Update,
                });

            return entities;
        }

        private class UpdatablePrinterNotificationsContact : IUpdatablePrinterNotificationsContact
        {
            public PrinterNotificationsContact Model { get; }
            private readonly string originalName;
            private readonly string originalAddress;
            private readonly int originalPrinterWorkerId;
            private readonly int? originalLocationId;
            private readonly bool originalIsDeleted;
            private readonly NotificationMessageType originalNotifications;
            private readonly DateTime now;

            public UpdatablePrinterNotificationsContact(PrinterNotificationsContact model, DateTime now)
            {
                Model = model;
                originalName = model.Name;
                originalAddress = model.Address;
                originalPrinterWorkerId = model.PrinterWorkerId;
                originalLocationId = model.LocationId;
                originalNotifications = Model.BaseNotificationsContact!.SubscribedNotifications;
                originalIsDeleted = IsDeleted;
                this.now = now;
            }

            public int Id => this.Model.Id;
            public string Name
            {
                get => Model.Name;
                set => Model.Name = value;
            }
            public string Address
            {
                get => Model.Address;
                set => Model.Address = value;
            }
            public int PrinterWorkerId
            {
                get => Model.PrinterWorkerId;
                set => Model.PrinterWorkerId = value;
            }
            public int? LocationId
            {
                get => Model.LocationId;
                set => Model.LocationId = value;
            }
            public NotificationMessageType Notifications
            {
                get => Model.BaseNotificationsContact!.SubscribedNotifications;
                set => Model.BaseNotificationsContact!.SubscribedNotifications = value;
            }

            public bool IsDeleted 
            {
                get => Model.DeletedDate.HasValue;
                set => Model.DeletedDate = value ? now : null;
            }

            public bool NameChanged => originalName != Model.Name;
            public bool AddressChanged => originalAddress != Model.Address;
            public bool PrinterWorkerIdChanged => originalPrinterWorkerId != Model.PrinterWorkerId;
            public bool LocationIdChanged => originalLocationId != Model.LocationId;
            public bool NotificationsChanged => originalNotifications != Model.BaseNotificationsContact!.SubscribedNotifications;
            public bool DeletedChange => originalIsDeleted != IsDeleted;
            public bool WasDeleted => !originalIsDeleted && IsDeleted;
            public bool HasChanges => NameChanged || AddressChanged || PrinterWorkerIdChanged || LocationIdChanged || NotificationsChanged || DeletedChange;
        }
    }
}