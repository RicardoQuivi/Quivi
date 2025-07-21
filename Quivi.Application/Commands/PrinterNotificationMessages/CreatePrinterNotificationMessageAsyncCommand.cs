using Quivi.Domain.Entities.Notifications;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.PrinterNotificationMessages;
using Quivi.Infrastructure.Abstractions.Events.Data.PrinterNotificationTargets;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.PrinterNotificationMessages
{
    public class CreatePrinterNotificationMessageAsyncCommand : ICommand<Task<IEnumerable<PrinterNotificationMessage>>>
    {
        public required GetPrinterNotificationsContactsCriteria Criteria { get; init; }
        public required Func<Task<string?>> GetContent { get; init; }

        public NotificationMessageType MessageType { get; init; }
    }

    public class CreatePrinterNotificationMessageAsyncCommandHandler : ICommandHandler<CreatePrinterNotificationMessageAsyncCommand, Task<IEnumerable<PrinterNotificationMessage>>>
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
            this.contactsRepository = contactsRepository;
            this.eventService = eventService;
        }

        public async Task<IEnumerable<PrinterNotificationMessage>> Handle(CreatePrinterNotificationMessageAsyncCommand command)
        {
            var printers = await GetPrinterTargets(command);
            if (printers.Any() == false)
                return [];

            var content = await command.GetContent();
            if (string.IsNullOrWhiteSpace(content))
                return [];

            var now = dateTimeProvider.GetUtcNow();
            var printerPerMerchant = printers.GroupBy(p => p.BaseNotificationsContact!.MerchantId);

            List<PrinterNotificationMessage> entities = new List<PrinterNotificationMessage>();
            foreach (var entry in printerPerMerchant)
            {
                var entity = new PrinterNotificationMessage
                {
                    MessageType = command.MessageType,
                    ContentType = PrinterMessageContentType.EscPos,
                    Content = content,
                    MerchantId = entry.Key,
                    PrinterMessageTargets = entry.Select(printer => new PrinterMessageTarget
                    {
                        CreatedDate = now,
                        ModifiedDate = now,
                        RequestedAt = null,
                        FinishedAt = null,
                        Status = AuditStatus.Pending,
                        PrinterNotificationsContactId = printer.Id,
                    }).ToList(),
                    CreatedDate = now,
                    ModifiedDate = now,
                };

                repository.Add(entity);
                entities.Add(entity);
            }
            if(entities.Any() == false)
                return [];

            await repository.SaveChangesAsync();
            foreach (var entity in entities)
            {
                await eventService.Publish(new OnPrinterNotificationMessageOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = entity.MerchantId,
                    Operation = EntityOperation.Create,
                });

                foreach (var target in entity.PrinterMessageTargets!)
                    await eventService.Publish(new OnPrinterMessageTargetOperationEvent
                    {
                        PrinterNotificationMessageId = entity.Id,
                        PrinterNotificationsContactId = target.PrinterNotificationsContactId,
                        MerchantId = entity.MerchantId,
                        Operation = EntityOperation.Create,
                    });
            }
            return entities;
        }

        private async Task<IEnumerable<PrinterNotificationsContact>> GetPrinterTargets(CreatePrinterNotificationMessageAsyncCommand command)
        {
            var printersQuery = await contactsRepository.GetAsync(command.Criteria with
            {
                IsDeleted = false,
                IncludeNotificationsContact = true,
            });
            return printersQuery;
        }
    }
}