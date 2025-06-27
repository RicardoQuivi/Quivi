using Quivi.Domain.Entities.Notifications;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.PrinterNotificationsContacts;
using Quivi.Infrastructure.Abstractions.Repositories;

namespace Quivi.Application.Commands.PrinterNotificationsContacts
{
    public class CreatePrinterNotificationsContactAsyncCommand : ICommand<Task<PrinterNotificationsContact?>>
    {
        public int MerchantId { get; init; }
        public required string Name { get; init; }
        public required string Address { get; init; }
        public NotificationMessageType Notifications { get; init; }
        public int PrinterWorkerId { get; init; }
        public int? LocationId { get; init; }
        public required Action OnInvalidName { get; init; }
        public required Action OnInvalidAddress { get; init; }
    }

    public class CreatePrinterNotificationsContactAsyncCommandHandler : ICommandHandler<CreatePrinterNotificationsContactAsyncCommand, Task<PrinterNotificationsContact?>>
    {
        private readonly IPrinterNotificationsContactsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public CreatePrinterNotificationsContactAsyncCommandHandler(IPrinterNotificationsContactsRepository repository,
                                                                        IDateTimeProvider dateTimeProvider,
                                                                        IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<PrinterNotificationsContact?> Handle(CreatePrinterNotificationsContactAsyncCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Name))
            {
                command.OnInvalidName();
                return null;
            }

            if (string.IsNullOrWhiteSpace(command.Address))
            {
                command.OnInvalidName();
                return null;
            }

            var now = dateTimeProvider.GetUtcNow();
            var entity = new PrinterNotificationsContact
            {
                Name = command.Name,
                Address = command.Address,
                PrinterWorkerId = command.PrinterWorkerId,
                LocationId = command.LocationId,
                BaseNotificationsContact = new NotificationsContact
                {
                    SubscribedNotifications = command.Notifications,
                    MerchantId = command.MerchantId,
                    CreatedDate = now,
                    ModifiedDate = now,
                },
                CreatedDate = now,
                ModifiedDate = now,
            };
            repository.Add(entity);

            await repository.SaveChangesAsync();

            await eventService.Publish(new OnPrinterNotificationsContactOperationEvent
            {
                Id = entity.Id,
                MerchantId = command.MerchantId,
                Operation = EntityOperation.Create,
            });
            return entity;
        }
    }
}