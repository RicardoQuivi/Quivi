using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Items;
using Quivi.Infrastructure.Pos.Facturalusa.Models.VatRates;
using System.Security.Cryptography;
using System.Text;

namespace Quivi.Infrastructure.Pos.Facturalusa.Commands
{
    public class GetOrCreateItemsAsyncCommand : AManageItemCommandBase
    {
        public GetOrCreateItemsAsyncCommand(IFacturalusaService facturalusaService) : base(facturalusaService)
        {
        }
    }

    public class UpsertItemsAsyncCommand : AManageItemCommandBase
    {
        public UpsertItemsAsyncCommand(IFacturalusaService facturalusaService) : base(facturalusaService)
        {
        }
    }

    public class AManageItemCommandBase : AFacturalusaAsyncCommand<IDictionary<ItemData, ReadonlyItem>>
    {
        public AManageItemCommandBase(IFacturalusaService facturalusaService) : base(facturalusaService)
        {
        }

        public required IEnumerable<ItemData> Items { get; set; }
    }

    public class ItemData
    {
        public bool IsGenericItem { get; set; }
        public ItemType Type { get; set; }

        private string? _reference;
        public string? Reference
        {
            get => _reference;
            set => _reference = value?.Trim();
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

    public class GetOrUpsertItemsAsyncCommandHandler : ICommandHandler<GetOrCreateItemsAsyncCommand, Task<IDictionary<ItemData, ReadonlyItem>>>,
                                                        ICommandHandler<UpsertItemsAsyncCommand, Task<IDictionary<ItemData, ReadonlyItem>>>
    {
        private readonly IFacturalusaCacheProvider cacheProvider;
        private readonly ICommandProcessor commandProcessor;

        public GetOrUpsertItemsAsyncCommandHandler(IFacturalusaCacheProvider cacheProvider, ICommandProcessor commandProcessor)
        {
            this.cacheProvider = cacheProvider;
            this.commandProcessor = commandProcessor;
        }

        public Task<IDictionary<ItemData, ReadonlyItem>> Handle(GetOrCreateItemsAsyncCommand command) => GetOrUpsert(command, false);

        public Task<IDictionary<ItemData, ReadonlyItem>> Handle(UpsertItemsAsyncCommand command) => GetOrUpsert(command, true);

        private async Task<IDictionary<ItemData, ReadonlyItem>> GetOrUpsert(AManageItemCommandBase command, bool isUpsert)
        {
            var vatRates = await GetVatRates(command);
            int unitId = await GetUnitId(command);

            var vatRatesValueIdRelation = vatRates.GroupBy(x => x.PercentageTax).ToDictionary(x => x.Key, x => x.Select(v => v.Id).First());

            var result = new Dictionary<ItemData, ReadonlyItem>();
            foreach (var item in command.Items)
            {
                var resultItem = await GetOrCreateItem(command.FacturalusaService, item, vatRatesValueIdRelation[item.VatRatePercentage], unitId, isUpsert);
                result.Add(item, resultItem);
            }
            return result;
        }

        private async Task<ReadonlyItem> GetOrCreateItem(IFacturalusaService service, ItemData item, int vatRateId, int unitId, bool overrideItem)
        {
            string itemReference = item.IsGenericItem ? AItem.DefaultCode : GetItemReference(item);

            return await cacheProvider.GetOrCreateItem(
                service.AccountUuid,
                itemReference,
                () => GetOrCreateItemViaApi(service, item, vatRateId, unitId),
                TimeSpan.FromDays(7),
                overrideItem
            );
        }

        private async Task<ReadonlyItem> GetOrCreateItemViaApi(IFacturalusaService service, ItemData item, int vatRateId, int unitId)
        {
            // If the item is a GenericItem so we get the pre-created Generic Item (Diversos)
            if (item.IsGenericItem)
            {
                var genericItemResponse = await service.GetItems(new GetItemsRequest
                {
                    FilterBy = ItemFilter.Reference,
                    Value = AItem.DefaultCode,
                });
                return genericItemResponse.Data.Single();
            }

            string itemReference = GetItemReference(item);

            var existingItemResponse = await service.GetItems(new GetItemsRequest
            {
                FilterBy = ItemFilter.Reference,
                Value = itemReference,
            });
            var existingItem = existingItemResponse.Data.FirstOrDefault(i => i.Reference == itemReference);

            if (existingItem == null)
            {
                // Create a new item
                var newItemResponse = await service.CreateItem(new CreateItemRequest
                {
                    Reference = itemReference,
                    Name = item.Name,
                    Type = item.Type,
                    VatId = vatRateId,
                    UnitId = unitId,
                    ExtraInfo = new AItem.ItemExtraInfo
                    {
                        CorrelationId = item.CorrelationId,
                    },
                });
                return newItemResponse.Data;
            }

            return existingItem;
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
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                return sBuilder.ToString();
            }
        }

        private async Task<IEnumerable<VatRate>> GetVatRates(AManageItemCommandBase command)
        {
            return await commandProcessor.Execute(new GetOrCreateVatRatesAsyncCommand(command.FacturalusaService)
            {
                VatRates = command.Items
                    .GroupBy(x => x.VatRatePercentage)
                    .Select(x => x.First())
                    .Select(item => new VatRateData
                    {
                        PercentageValue = item.VatRatePercentage,
                    }),
            });
        }

        private async Task<int> GetUnitId(AManageItemCommandBase command)
        {
            var response = await commandProcessor.Execute(new GetOrCreateUnitAsyncCommand(command.FacturalusaService)
            {
                Name = "Unidades",
                Symbol = "uni",
            });
            return response.Id;
        }
    }
}
