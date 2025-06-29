using Quivi.Domain.Entities.Notifications;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.PrinterNotificationMessages;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.PrinterNotificationMessages
{
    public class CreatePrinterNotificationMessageAsyncCommand : ICommand<Task<PrinterNotificationMessage>>
    {
        public int MerchantId { get; init; }
        public NotificationMessageType MessageType { get; init; }
        public PrinterMessageContentType ContentType { get; init; }
        public required string Content { get; init; }
        public required IEnumerable<int> PrinterNotificationsContactIds { get; init; }
    }

    public class CreatePrinterNotificationMessageAsyncCommandHandler : ICommandHandler<CreatePrinterNotificationMessageAsyncCommand, Task<PrinterNotificationMessage>>
    {
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IPrinterNotificationsContactsRepository contactsRepository;
        private readonly IPrinterNotificationMessagesRepository repository;
        private readonly IEventService eventService;

        public CreatePrinterNotificationMessageAsyncCommandHandler(IDateTimeProvider dateTimeProvider,
                                                                    IPrinterNotificationsContactsRepository contactsRepository,
                                                                    IPrinterNotificationMessagesRepository repository,
                                                                    IEventService eventService)
        {
            this.dateTimeProvider = dateTimeProvider;
            this.repository = repository;
            this.eventService = eventService;
        }

        public async Task<PrinterNotificationMessage> Handle(CreatePrinterNotificationMessageAsyncCommand command)
        {
            var contactsQuery = await contactsRepository.GetAsync(new GetPrinterNotificationsContactsCriteria
            {
                MerchantIds = [command.MerchantId],
                Ids = command.PrinterNotificationsContactIds,
                PageIndex = 0,
                PageSize = 0,
            });
            if (contactsQuery.TotalItems != command.PrinterNotificationsContactIds.Count())
                throw new UnauthorizedAccessException();

            var now = dateTimeProvider.GetUtcNow();
            var entity = new PrinterNotificationMessage
            {
                MessageType = command.MessageType,
                ContentType = command.ContentType,
                Content = command.Content,
                MerchantId = command.MerchantId,
                PrinterMessageTargets = command.PrinterNotificationsContactIds.Select(cId => new PrinterMessageTarget
                {
                    CreatedDate = now,
                    ModifiedDate = now,
                    RequestedAt = null,
                    FinishedAt = null,
                    Status = AuditStatus.Pending,
                    PrinterNotificationsContactId = cId,
                }).ToList(),
                CreatedDate = now,
                ModifiedDate = now,
            };

            repository.Add(entity);
            await repository.SaveChangesAsync();

            await eventService.Publish(new OnPrinterNotificationMessageOperationEvent
            {
                Id = entity.Id,
                MerchantId = command.MerchantId,
                Operation = EntityOperation.Create,
            });
            return entity;
        }
    }
}
