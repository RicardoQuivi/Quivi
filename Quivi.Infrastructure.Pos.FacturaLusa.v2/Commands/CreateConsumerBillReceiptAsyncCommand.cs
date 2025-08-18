using FacturaLusa.v2.Dtos;
using FacturaLusa.v2.Dtos.Requests.Sales;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Models;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Commands
{
    public class CreateConsumerBillReceiptAsyncCommand : AFacturaLusaAsyncCommand<Sale>
    {
        public required CreateConsumerBillData Data { get; init; }
        public DocumentFormat? Format { get; init; }

        public class CreateConsumerBillData
        {
            public required DateTime CreatedDateLocal { get; init; }
            public string? VatNumber { get; init; }
            public VatType VatType { get; init; }
            public string? Observations { get; init; }
            public required string Reference { get; init; }
            public required string SerieCode { get; init; }
            public required IEnumerable<SaleDocumentItem> Items { get; init; }
        }
    }

    public class CreateConsumerBillReceiptAsyncCommandHandler : AGetOrCreateSaleCommandHandler, ICommandHandler<CreateConsumerBillReceiptAsyncCommand, Task<Sale>>
    {
        private readonly ICommandProcessor commandProcessor;
        private readonly IFacturaLusaSettings settings;

        public CreateConsumerBillReceiptAsyncCommandHandler(
            ICommandProcessor commandProcessor,
            IFacturaLusaSettings settings,
            IFacturaLusaCacheProvider cacheProvider) : base(cacheProvider)
        {
            this.commandProcessor = commandProcessor;
            this.settings = settings;
        }

        private string GetItemKey(string reference, string correlationId) => $"{reference}_{correlationId}";

        public async Task<Sale> Handle(CreateConsumerBillReceiptAsyncCommand command)
        {
            var items = await GetItems(command);
            var itemsCodeIdRelation = items.ToDictionary(x => GetItemKey(x.Key.Reference!, x.Key.CorrelationId), x => x.Value.Id);
            var genericItem = new Lazy<Item>(() => items.Select(x => x.Value).First(x => x.Reference == "Diversos"));

            var vatRates = await GetVatRates(command);
            var vatRatesValueIdRelation = vatRates.GroupBy(v => v.TaxPercentage).ToDictionary(x => x.Key, x => x.First().Id);

            var finalConsumer = await GetFinalConsumer(command);
            var currency = await GetCurrency(command);
            var serie = await GetSerie(command);

            var newSaleResponse = await command.Service.CreateSale(new CreateSaleRequest
            {
                Status = SaleStatus.Final,
                CurrencyId = currency.Id,
                SerieId = serie.Id,
                IssueDate = command.Data.CreatedDateLocal,
                CustomerId = finalConsumer.Id,
                VatNumber = Set(command.Data.VatNumber, finalConsumer.VatNumber!),
                DocumentType = DocumentType.ConsumerBill,
                VatType = command.Data.VatType,
                Observations = command.Data.Observations,
                Items = command.Data.Items.Select(item => new CreateSaleItem
                {
                    Id = item.IsGeneric ? genericItem.Value.Id : itemsCodeIdRelation[GetItemKey(item.Reference, item.CorrelationId)],
                    Price = item.Price,
                    Quantity = item.Quantity,
                    VatRateId = vatRatesValueIdRelation[item.TaxPercentage],
                    Details = item.Description,
                    Discount = item.DiscountPercentage,
                    VatExemption = item.TaxPercentage == 0 ? VatExemptionType.GenericExemption : (VatExemptionType?)null,
                }),
                Reference = command.Data.Reference,
                Format = command.Format,
                ForcePrint = command.Format.HasValue,
            });

            await PostSaleCreation(command.Service, newSaleResponse);

            return newSaleResponse;
        }

        private string Set(string? value, string defaultValue) => string.IsNullOrWhiteSpace(value) ? defaultValue : value;

        private Task<Currency> GetCurrency(CreateConsumerBillReceiptAsyncCommand command)
        {
            return commandProcessor.Execute(new GetOrCreateCurrencyAsyncCommand
            {
                Service = command.Service,
                IsoCode = "EUR",
                Name = "Euro",
                Symbol = "€",
            });
        }

        private Task<Customer> GetFinalConsumer(CreateConsumerBillReceiptAsyncCommand command)
        {
            return commandProcessor.Execute(new GetOrCreateCustomerAsyncCommand
            {
                Service = command.Service,
                IsFinalConsumer = true,
                VatNumber = string.Empty,
            });
        }

        private Task<Serie> GetSerie(CreateConsumerBillReceiptAsyncCommand command)
        {
            return commandProcessor.Execute(new GetOrCreateSerieAsyncCommand
            {
                Service = command.Service,
                SerieName = command.Data.SerieCode,
                ExpirationYear = 2100,
                CommunicateIfNew = settings.CommunicateSeries,
            });
        }

        private Task<IDictionary<ItemData, Item>> GetItems(CreateConsumerBillReceiptAsyncCommand command)
        {
            return commandProcessor.Execute(new GetOrCreateItemsAsyncCommand
            {
                Service = command.Service,
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

        private Task<IEnumerable<VatRate>> GetVatRates(CreateConsumerBillReceiptAsyncCommand command)
        {
            return commandProcessor.Execute(new GetOrCreateVatRatesAsyncCommand
            {
                Service = command.Service,
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
