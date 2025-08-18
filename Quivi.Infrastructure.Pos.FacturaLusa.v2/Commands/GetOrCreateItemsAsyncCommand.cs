using FacturaLusa.v2.Dtos;
using FacturaLusa.v2.Dtos.Requests.Items;
using FacturaLusa.v2.Exceptions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Commands
{
    public class GetOrCreateItemsAsyncCommand : AManageItemCommandBase
    {
    }

    public class UpsertItemsAsyncCommand : AManageItemCommandBase
    {
    }

    public class AManageItemCommandBase : AFacturaLusaAsyncCommand<IDictionary<ItemData, Item>>
    {
        public required IEnumerable<ItemData> Items { get; set; }
    }

    public class ItemData
    {
        public bool IsGenericItem { get; set; }
        public ItemType Type { get; set; }

        private string _reference = string.Empty;
        public required string Reference
        {
            get => _reference;
            set => _reference = value.Trim();
        }

        public required string CorrelationId { get; set; }
        public required string Name { get; set; }
        public string? Details { get; set; }
        private decimal _vatRatePercentage;
        public decimal VatRatePercentage
        {
            get => _vatRatePercentage;
            set => _vatRatePercentage = Math.Round(value, 2);
        }

        public bool IsDeleted { get; set; }
    }

    public class GetOrUpsertItemsAsyncCommandHandler : ICommandHandler<GetOrCreateItemsAsyncCommand, Task<IDictionary<ItemData, Item>>>,
                                                        ICommandHandler<UpsertItemsAsyncCommand, Task<IDictionary<ItemData, Item>>>
    {
        public static readonly string DefaultCode = "Diversos";

        private readonly IFacturaLusaCacheProvider cacheProvider;
        private readonly ICommandProcessor commandProcessor;

        public GetOrUpsertItemsAsyncCommandHandler(IFacturaLusaCacheProvider cacheProvider, ICommandProcessor commandProcessor)
        {
            this.cacheProvider = cacheProvider;
            this.commandProcessor = commandProcessor;
        }

        public Task<IDictionary<ItemData, Item>> Handle(GetOrCreateItemsAsyncCommand command) => GetOrUpsert(command, false);

        public Task<IDictionary<ItemData, Item>> Handle(UpsertItemsAsyncCommand command) => GetOrUpsert(command, true);

        private async Task<IDictionary<ItemData, Item>> GetOrUpsert(AManageItemCommandBase command, bool isUpsert)
        {
            var vatRates = await GetVatRates(command);
            long unitId = await GetUnitId(command);

            var vatRatesValueIdRelation = vatRates.GroupBy(x => x.TaxPercentage).ToDictionary(x => x.Key, x => x.Select(v => v.Id).First());

            var result = new Dictionary<ItemData, Item>();
            foreach (var item in command.Items)
            {
                var resultItem = await GetOrCreateItem(command.Service, item, vatRatesValueIdRelation[item.VatRatePercentage], unitId, isUpsert);
                result.Add(item, resultItem);
            }
            return result;
        }

        private async Task<Item> GetOrCreateItem(IFacturaLusaService service, ItemData item, long vatRateId, long unitId, bool overrideItem)
        {
            string itemReference = item.IsGenericItem ? DefaultCode : GetItemReference(item);

            return await cacheProvider.GetOrCreateItem(
                service.AccountUuid,
                itemReference,
                () => GetOrCreateItemViaApi(service, item, vatRateId, unitId),
                TimeSpan.FromDays(7),
                overrideItem
            );
        }

        private async Task<Item> GetOrCreateItemViaApi(IFacturaLusaService service, ItemData item, long vatRateId, long unitId)
        {
            // If the item is a GenericItem so we get the pre-created Generic Item (Diversos)
            if (item.IsGenericItem)
            {
                var genericItemResponse = await service.SearchItem(new SearchItemRequest
                {
                    Field = SearchField.Reference,
                    Value = DefaultCode,
                });
                return genericItemResponse;
            }

            string itemReference = GetItemReference(item);
            try
            {
                var existingItemResponse = await service.SearchItem(new SearchItemRequest
                {
                    Field = SearchField.Reference,
                    Value = itemReference,
                });
                return existingItemResponse;
            }
            catch (FacturaLusaException ex) when (ex.ErrorType == ErrorType.EntityXNotExists)
            {
                var newItemResponse = await service.CreateItem(new CreateItemRequest
                {
                    Reference = itemReference,
                    Description = item.Name,
                    Type = item.Type,
                    VatRateId = vatRateId,
                    UnitId = unitId,
                });
                return newItemResponse;
            }
        }

        private string GetItemReference(ItemData item) => item.Reference ?? BuildItemKey(item.CorrelationId, item.Name);

        /// <summary>
        /// Builds as hash between CorrelationId and Name because we cannot change the name of an item that was already billed.
        /// </summary>
        /// <param name="correlationId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private string BuildItemKey(string correlationId, string name)
        {
            string combinedString = $"{correlationId}_{name.Trim()}";
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(combinedString));

                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                    sBuilder.Append(data[i].ToString("x2"));

                return sBuilder.ToString();
            }
        }

        private async Task<IEnumerable<VatRate>> GetVatRates(AManageItemCommandBase command)
        {
            return await commandProcessor.Execute(new GetOrCreateVatRatesAsyncCommand
            {
                Service = command.Service,
                VatRates = command.Items
                    .GroupBy(x => x.VatRatePercentage)
                    .Select(x => x.First())
                    .Select(item => new VatRateData
                    {
                        PercentageValue = item.VatRatePercentage,
                    }),
            });
        }

        private async Task<long> GetUnitId(AManageItemCommandBase command)
        {
            var response = await commandProcessor.Execute(new GetOrCreateUnitAsyncCommand
            {
                Service = command.Service,
                Name = "Unidades",
                Symbol = "uni",
            });
            return response.Id;
        }
    }
}
