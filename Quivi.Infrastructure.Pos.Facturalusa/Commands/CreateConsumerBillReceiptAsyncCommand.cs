using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;
using Quivi.Infrastructure.Pos.Facturalusa.Configurations;
using Quivi.Infrastructure.Pos.Facturalusa.Mappers;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Customers;
using Quivi.Infrastructure.Pos.Facturalusa.Models.External;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Items;
using Quivi.Infrastructure.Pos.Facturalusa.Models.VatRates;

namespace Quivi.Infrastructure.Pos.Facturalusa.Commands
{
    public class CreateConsumerBillReceiptAsyncCommand : AFacturalusaAsyncCommand<ConsumerBill>
    {
        public CreateConsumerBillReceiptAsyncCommand(IFacturalusaService facturalusaService) : base(facturalusaService)
        {
        }

        public ConsumerBill Data { get; set; }

        public Models.Sales.DownloadSaleFormat? IncludePdfFileInFormat { get; set; }
    }

    public class CreateConsumerBillReceiptAsyncCommandHandler : AGetOrCreateSaleCommandHandler, ICommandHandler<CreateConsumerBillReceiptAsyncCommand, Task<ConsumerBill>>
    {
        private readonly ICommandProcessor commandProcessor;
        private readonly IFacturalusaSettings settings;

        public CreateConsumerBillReceiptAsyncCommandHandler(
            ICommandProcessor commandProcessor,
            IFacturalusaSettings settings, 
            IFacturalusaCacheProvider cacheProvider) : base(cacheProvider)
        {
            this.commandProcessor = commandProcessor;
            this.settings = settings;
        }

        private string GetItemKey(string reference, string correlationId) => $"{reference}_{correlationId}";

        public async Task<ConsumerBill> Handle(CreateConsumerBillReceiptAsyncCommand command)
        {
            var items = await GetItems(command);
            var itemsCodeIdRelation = items.ToDictionary(x => GetItemKey(x.Key.Reference, x.Key.CorrelationId), x => x.Value.Id);
            var genericItem = new Lazy<ReadonlyItem>(() => items.Select(x => x.Value).First(x => x.IsGenericItem));

            var vatRates = await GetVatRates(command);
            var vatRatesValueIdRelation = vatRates.GroupBy(v => v.PercentageTax).ToDictionary(x => x.Key, x => x.First().Id);

            var finalConsumer = await GetFinalConsumer(command);

            var newSaleResponse = await command.FacturalusaService.CreateSale(new Models.Sales.CreateSaleRequest
            {
                Status = Models.Sales.SaleStatus.Final,
                CurrencyId = await GetCurrencyId(command),
                SerieId = await GetSerieId(command),
                IssueDay = command.Data.CreatedDateLocal,
                CustomerId = finalConsumer.Id,
                CustomerVatNumber = Set(command.Data.CustomerVatNumber, finalConsumer.VatNumber),
                DocumentType = Models.DocumentTypes.DocumentType.ConsumerBill,
                VatType = SaleMapper.Convert(command.Data.PricesType),
                Notes = command.Data.Notes,
                Items = command.Data.Items.Select(item => new Models.Sales.WriteonlySaleItem
                {
                    ItemId = item.IsGeneric ? genericItem.Value.Id : itemsCodeIdRelation[GetItemKey(item.Reference, item.CorrelationId)],
                    Price = item.Price,
                    Quantity = item.Quantity,
                    VatRateId = vatRatesValueIdRelation[item.TaxPercentage],
                    Description = item.Description,
                    DiscountPercentage = item.DiscountPercentage,
                    VatRateExemption = item.TaxPercentage == 0 ? VatRateExemptionType.GenericExemption : (VatRateExemptionType?)null,
                }),
                IncludePdfFileUrl = command.IncludePdfFileInFormat.HasValue,
                Format = command.IncludePdfFileInFormat ?? Models.Sales.DownloadSaleFormat.A4,
            });

            await PostSaleCreation(command.FacturalusaService, newSaleResponse);

            return SaleMapper.ConvertToBudgetReceipt(newSaleResponse.Data);
        }

        private string? Set(string? value, string? defaultValue) => string.IsNullOrWhiteSpace(value) ? defaultValue : value;

        private async Task<int> GetCurrencyId(CreateConsumerBillReceiptAsyncCommand command)
        {
            var result = await commandProcessor.Execute(new GetOrCreateCurrencyAsyncCommand(command.FacturalusaService)
            {
                IsoCode = "EUR",
                Name = "Euro",
                Symbol = "€",
            });
            return result.Id;
        }

        private async Task<ReadOnlyCustomer> GetFinalConsumer(CreateConsumerBillReceiptAsyncCommand command)
        {
            var result = await commandProcessor.Execute(new GetOrCreateCustomerAsyncCommand(command.FacturalusaService)
            {
                IsFinalConsumer = true,
            });
            return result;
        }

        private async Task<long> GetSerieId(CreateConsumerBillReceiptAsyncCommand command)
        {
            var result = await commandProcessor.Execute(new GetOrCreateSerieAsyncCommand(command.FacturalusaService)
            {
                SerieName = command.Data.SerieCode,
                ExpirationYear = 2100,
                CommunicateIfNew = settings.CommunicateSeries,
            });
            return result.Id;
        }

        private async Task<IDictionary<ItemData, ReadonlyItem>> GetItems(CreateConsumerBillReceiptAsyncCommand command)
        {
            return await commandProcessor.Execute(new GetOrCreateItemsAsyncCommand(command.FacturalusaService)
            {
                Items = command.Data.Items.GroupBy(x => GetItemKey(x.Reference, x.CorrelationId)).Select(x => x.First()).Select(item => new ItemData
                {
                    Reference = item.Reference,
                    CorrelationId = item.CorrelationId,
                    IsGenericItem = item.IsGeneric,
                    Type = item.Type,
                    Name = item.Name,
                    VatRatePercentage = item.TaxPercentage,
                    Details = item.Description,
                }),
            });
        }

        private async Task<IEnumerable<VatRate>> GetVatRates(CreateConsumerBillReceiptAsyncCommand command)
        {
            return await commandProcessor.Execute(new GetOrCreateVatRatesAsyncCommand(command.FacturalusaService)
            {
                VatRates = command.Data.Items
                    .GroupBy(x => x.TaxPercentage)
                    .Select(x => x.First())
                    .Select(item => new VatRateData
                    {
                        PercentageValue = item.TaxPercentage,
                    }),
            });
        }
    }
}
