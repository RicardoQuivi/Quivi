using Quivi.Application.Commands.MenuItems;
using Quivi.Application.Extensions;
using Quivi.Application.Queries.Merchants;
using Quivi.Application.Queries.PrinterNotificationsContacts;
using Quivi.Domain.Entities.Notifications;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.PrinterNotificationMessages;
using Quivi.Infrastructure.Abstractions.Events.Data.PrinterNotificationTargets;
using Quivi.Infrastructure.Abstractions.Pos.EscPos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.PreparationGroups
{
    public class PrintPreparationGroupAsyncCommand : ICommand<Task>
    {
        public int MerchantId { get; init; }
        public int PreparationGroupId { get; init; }
        public int? LocationId { get; init; }
    }

    public class PrintPreparationGroupAsyncCommandHandler : ICommandHandler<PrintPreparationGroupAsyncCommand, Task>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IIdConverter idConverter;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IPreparationGroupsRepository groupsRepository;
        private readonly IPrinterNotificationMessagesRepository repository;
        private readonly IEscPosPrinterService printerService;
        private readonly IEventService eventService;

        private enum PreparationType
        {
            Order = 0,
            Cancellation = 1,
        }

        public PrintPreparationGroupAsyncCommandHandler(IQueryProcessor queryProcessor,
                                                        IPreparationGroupsRepository preparationGroupsRepository,
                                                        IIdConverter idConverter,
                                                        IDateTimeProvider dateTimeProvider,
                                                        IEventService eventService,
                                                        IPrinterNotificationMessagesRepository repository,
                                                        IEscPosPrinterService printerService)
        {
            this.queryProcessor = queryProcessor;
            this.idConverter = idConverter;
            this.dateTimeProvider = dateTimeProvider;
            this.groupsRepository = preparationGroupsRepository;
            this.eventService = eventService;
            this.repository = repository;
            this.printerService = printerService;
        }

        public async Task Handle(PrintPreparationGroupAsyncCommand command)
        {
            //TODO: This should be a query instead of this
            var groupsQuery = await groupsRepository.GetAsync(new GetPreparationGroupsCriteria
            {
                MerchantIds = [command.MerchantId],
                Ids = [command.PreparationGroupId],
                States = [PreparationGroupState.Committed],
                IncludePreparationGroupItemMenuItems = true,
                IncludeSessionChannel = true,
                IncludeSessionChannelProfile = true,
                IncludeOrders = true,
                IncludeOrdersSequence = true,
                IncludeOrderFields = true,
                PageIndex = 0,
                PageSize = 1,
            });

            var group = groupsQuery.SingleOrDefault();
            if (group == null)
                return;

            await GenerateMessages(command, group);
        }

        private async Task GenerateMessages(PrintPreparationGroupAsyncCommand command, PreparationGroup group)
        {
            var defaultPrinterQuery = await queryProcessor.Execute(new GetPrinterNotificationsContactsAsyncQuery
            {
                MerchantIds = [command.MerchantId],
                MessageTypes = [NotificationMessageType.NewPreparationRequest],
                IsDeleted = false,
                IncludeNotificationsContact = true,

                PageIndex = 0,
                PageSize = 1,
            });
            var defaultPrinter = defaultPrinterQuery.SingleOrDefault();
            if (defaultPrinter == null)
                return;

            var merchantQuery = await queryProcessor.Execute(new GetMerchantsAsyncQuery
            {
                Ids = [command.MerchantId],
                PageIndex = 0,
                PageSize = 1,
            });
            var merchant = merchantQuery.Single();

            var itemPrintersDictionary = await queryProcessor.Execute(new GetMenuItemsPrintersAsyncQuery
            {
                MenuItemIds = group.PreparationGroupItems!.Select(s => s.MenuItemId),
                SourceLocationId = command.LocationId,
            });

            var notes = GetOrderNotes(group);
            if (string.IsNullOrWhiteSpace(group.AdditionalNote) == false)
                notes = notes.Prepend(new KeyValuePair<string, string>("Notas", $"\n{group.AdditionalNote}\n"));

            var entries = group.PreparationGroupItems!.Where(s => s.ParentPreparationGroupItemId.HasValue == false).Select(item => new
            {
                Item = item,
                PrinterIds = item.Extras.OrEmpty().Select(e => itemPrintersDictionary[e.MenuItemId]).Append(itemPrintersDictionary[item.MenuItemId].OrIfEmpty(
                [
                    defaultPrinter.Id,
                ])).SelectMany(s => s).ToHashSet(),
            }).SelectMany(r => r.PrinterIds.Select(t => new
            {
                Item = r.Item,
                PrinterId = t,
            })).GroupBy(i => new
            {
                i.PrinterId,
                PreparationType = i.Item.OriginalQuantity >= 0 ? PreparationType.Order : PreparationType.Cancellation
            });

            var merchantNow = dateTimeProvider.GetNow(merchant.TimeZone);
            var now = dateTimeProvider.GetUtcNow();
            var addedEntities = new List<PrinterNotificationMessage>();
            foreach (var entry in entries)
            {
                var printerId = entry.Key.PrinterId;
                var preparationType = entry.Key.PreparationType;
                var document = printerService.Get(new PreparationRequestParameters
                {
                    Title = preparationType == PreparationType.Order ? "Pedido de preparação" : "Cancelamento de preparação",
                    OrderPlaceholder = GetOrderPlaceholder(group),
                    Timestamp = merchantNow,
                    ChannelPlaceholder = $"{group.Session!.Channel!.ChannelProfile!.Name} {group.Session.Channel.Identifier}",
                    SessionPlaceholder = idConverter.ToPublicId(group.SessionId),
                    AdditionalInfo = notes.ToList(),
                    Items = entry.Select(it => new PreparationRequestItem
                    {
                        Name = it.Item.MenuItem!.Name,
                        Quantity = it.Item.OriginalQuantity,
                        Modifiers = it.Item.Extras.OrEmpty()
                                            .Where(m =>
                                            {
                                                var printers = itemPrintersDictionary[m.MenuItemId].OrIfEmpty(
                                                [
                                                    printerId,
                                                ]).ToHashSet();
                                                return printers.Contains(printerId);
                                            })
                                            .Select(c => new BasePreparationRequestItem
                                            {
                                                Name = c.MenuItem!.Name,
                                                Quantity = c.OriginalQuantity,
                                            }).ToList(),
                    }).ToList(),
                });

                var entity = new PrinterNotificationMessage
                {
                    MessageType = NotificationMessageType.NewPreparationRequest,
                    ContentType = PrinterMessageContentType.EscPos,
                    Content = document,
                    MerchantId = command.MerchantId,
                    PrinterMessageTargets =
                    [
                        new PrinterMessageTarget
                        {
                            CreatedDate = now,
                            ModifiedDate = now,
                            RequestedAt = null,
                            FinishedAt = null,
                            Status = AuditStatus.Pending,
                            PrinterNotificationsContactId = entry.Key.PrinterId,
                        },
                    ],
                    CreatedDate = now,
                    ModifiedDate = now,
                };

                addedEntities.Add(entity);
                repository.Add(entity);
            }

            if (addedEntities.Any() == false)
                return;

            await repository.SaveChangesAsync();

            foreach(var entity in addedEntities)
            {
                await eventService.Publish(new OnPrinterNotificationMessageOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = command.MerchantId,
                    Operation = EntityOperation.Create,
                });

                foreach (var target in entity.PrinterMessageTargets!)
                    await eventService.Publish(new OnPrinterMessageTargetOperationEvent
                    {
                        PrinterNotificationMessageId = entity.Id,
                        PrinterNotificationsContactId = target.PrinterNotificationsContactId,
                        MerchantId = command.MerchantId,
                        Operation = EntityOperation.Create,
                    });
            }
        }

        private string? GetOrderPlaceholder(PreparationGroup group)
        {
            if(group.Orders?.Count != 1)
                return null;
            var order = group.Orders.Single();
            return $"Pedido {order.OrderSequence?.SequenceNumber.ToString() ?? idConverter.ToPublicId(order.Id)}";
        }

        private static IEnumerable<KeyValuePair<string, string>> GetOrderNotes(PreparationGroup group)
        {
            foreach (var field in group.Orders!.SelectMany(m => m.OrderAdditionalInfos.OrEmpty()).Where(f => f.OrderConfigurableField!.PrintedOn == PrintedOn.PreparationRequest))
            {
                var configurableField = field.OrderConfigurableField!;
                if (string.IsNullOrWhiteSpace(field.Value) == false)
                    yield return new KeyValuePair<string, string>(configurableField.Name, configurableField.Type == FieldType.BigString ? $"\n{field.Value}\n" : field.Value);
            }
        }
    }
}
