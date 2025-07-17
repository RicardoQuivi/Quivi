using Quivi.Application.Queries.MenuItems;
using Quivi.Application.Queries.PrinterNotificationsContacts;
using Quivi.Domain.Entities.Notifications;
using Quivi.Infrastructure.Abstractions.Cqrs;

namespace Quivi.Application.Commands.MenuItems
{
    public class GetMenuItemsPrintersAsyncQuery : IQuery<Task<IReadOnlyDictionary<int, IEnumerable<int>>>>
    {
        /// <summary>
        /// The MenuItemIds from which we want to know the target printer
        /// </summary>
        public required IEnumerable<int> MenuItemIds { get; init; }
        /// <summary>
        /// The PreparationLocation Id of the request. If an item doesn't have a PreparationLocation assigned, then
        /// the target printer used will be the printer assigned to the provided SourceLocationId. If no SourceLocationId is provided,
        /// then the item will have no printer assigned.
        /// </summary>
        public int? SourceLocationId { get; init; }
    }

    public class GetMenuItemsPrintersAsyncQueryHandler : IQueryHandler<GetMenuItemsPrintersAsyncQuery, Task<IReadOnlyDictionary<int, IEnumerable<int>>>>
    {
        private readonly IQueryProcessor queryProcessor;

        public GetMenuItemsPrintersAsyncQueryHandler(IQueryProcessor queryProcessor)
        {
            this.queryProcessor = queryProcessor;
        }

        public async Task<IReadOnlyDictionary<int, IEnumerable<int>>> Handle(GetMenuItemsPrintersAsyncQuery query)
        {
            var itemsQuery = await queryProcessor.Execute(new GetMenuItemsAsyncQuery
            {
                Ids = query.MenuItemIds,
                IsDeleted = null,
                PageSize = null,
            });

            var preparationLocationsIds = itemsQuery.Where(s => s.LocationId.HasValue).Select(s => s.LocationId!.Value);
            if (query.SourceLocationId.HasValue)
                preparationLocationsIds = preparationLocationsIds.Prepend(query.SourceLocationId.Value);

            var availablePrintersPerLocationQuery = await queryProcessor.Execute(new GetPrinterNotificationsContactsAsyncQuery
            {
                LocationIds = preparationLocationsIds,
                MessageTypes = [NotificationMessageType.NewPreparationRequest],
                IsDeleted = false,
                PageSize = null,
            });
            var availablePrintersPerLocation = availablePrintersPerLocationQuery.GroupBy(p => p.LocationId!.Value).ToDictionary(p => p.Key, p => p.AsEnumerable());

            var result = new Dictionary<int, IEnumerable<int>>();
            foreach (var item in itemsQuery)
            {
                if (item.LocationId.HasValue == false)
                {
                    if (query.SourceLocationId.HasValue == false)
                    {
                        result.Add(item.Id, Enumerable.Empty<int>());
                        continue;
                    }

                    if (availablePrintersPerLocation.TryGetValue(query.SourceLocationId.Value, out var defaultLocationPrinters))
                    {
                        result.Add(item.Id, defaultLocationPrinters.Select(p => p.Id));
                        continue;
                    }

                    //This means the item has a location assigned but no printer available.
                    //What to do? Line bellow?
                    result.Add(item.Id, Enumerable.Empty<int>());
                    continue;
                }

                if (availablePrintersPerLocation.TryGetValue(item.LocationId.Value, out var printers))
                {
                    result.Add(item.Id, printers.Select(p => p.Id));
                    continue;
                }

                result.Add(item.Id, Enumerable.Empty<int>());
            }
            return result;
        }
    }
}
