using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;
using Quivi.Infrastructure.Pos.Facturalusa.Configurations;
using Quivi.Infrastructure.Pos.Facturalusa.Mappers;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Customers;
using Quivi.Infrastructure.Pos.Facturalusa.Models.External;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Items;
using Quivi.Infrastructure.Pos.Facturalusa.Models.VatRates;
using Quivi.Infrastructure.Pos.Facturalusa.Queries;

namespace Quivi.Infrastructure.Pos.Facturalusa.Commands
{
    public class CreateCreditNoteAsyncCommand : AFacturalusaAsyncCommand<CreditNote>
    {
        public CreateCreditNoteAsyncCommand(IFacturalusaService facturalusaService) : base(facturalusaService)
        {

        }

        public required CreditNote Data { get; set; }
        
        public Models.Sales.DownloadSaleFormat? IncludePdfFileInFormat { get; set; }
    }

    public class CreateCreditNoteAsyncCommandHandler : AGetOrCreateSaleCommandHandler, ICommandHandler<CreateCreditNoteAsyncCommand, Task<CreditNote>>
    {
        private readonly ICommandProcessor _commandProcessor;
        private readonly IQueryProcessor _queryProcessor;
        private readonly IFacturalusaSettings settings;

        public CreateCreditNoteAsyncCommandHandler(
            ICommandProcessor commandProcessor, 
            IQueryProcessor queryProcessor,
            IFacturalusaSettings settings, 
            IFacturalusaCacheProvider cacheProvider) : base(cacheProvider)
        {
            _commandProcessor = commandProcessor;
            _queryProcessor = queryProcessor;
            this.settings = settings;
        }

        private string GetItemKey(string reference, string correlationId) => $"{reference}_{correlationId}";

        public async Task<CreditNote> Handle(CreateCreditNoteAsyncCommand command)
        {
            var items = await GetItems(command);
            var itemsCodeIdRelation = items.ToDictionary(x => GetItemKey(x.Key.Reference, x.Key.CorrelationId), x => x.Value.Id);
            var genericItem = new Lazy<ReadonlyItem>(() => items.Select(x => x.Value).First(x => x.IsGenericItem));

            var vatRates = await GetVatRates(command);
            var vatRatesValueIdRelation = vatRates.GroupBy(v => v.PercentageTax).ToDictionary(x => x.Key, x => x.First().Id);

            var customer = await GetCustomer(command);

            var newSaleResponse = await command.FacturalusaService.CreateSale(new Models.Sales.CreateSaleRequest
            {
                Status = Models.Sales.SaleStatus.Final,
                ReferencedSaleId = await GetReferenceSaleId(command),
                CurrencyId = await GetCurrencyId(command),
                PaymentMethodId = await GetPaymentMethodId(command),
                PaymentConditionId = await GetPaymentConditionId(command),
                SerieId = await GetSerieId(command),
                IssueDay = command.Data.CreatedDateLocal,
                CustomerId = customer.Id,
                CustomerVatNumber = Set(command.Data.Customer.VatNumber, customer.VatNumber),
                CustomerAddress = Set(command.Data.Customer.Address, customer.Address),
                CustomerCityName = Set(command.Data.Customer.CityName, customer.CityName),
                CustomerPostalCode = Set(command.Data.Customer.PostalCode, customer.PostalCode),
                CustomerCountryName = Set(command.Data.Customer.CountryName, customer.CountryName),
                DocumentType = Models.DocumentTypes.DocumentType.CreditNote,
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

            return SaleMapper.ConvertToCreditNote(newSaleResponse.Data);
        }

        private string? Set(string? value, string? defaultValue) => string.IsNullOrWhiteSpace(value) ? defaultValue: value;

        private async Task<long> GetReferenceSaleId(CreateCreditNoteAsyncCommand command)
        {
            return await _queryProcessor.Execute(new GetFacturalusaSaleIdAsyncQuery(command.FacturalusaService)
            {
                DocumentId = command.Data.RelatedDocumentId,
            });
        }

        private async Task<int> GetCurrencyId(CreateCreditNoteAsyncCommand command)
        {
            var result = await _commandProcessor.Execute(new GetOrCreateCurrencyAsyncCommand(command.FacturalusaService)
            {
                IsoCode = "EUR",
                Name = "Euro",
                Symbol = "€",
            });
            return result.Id;
        }

        private async Task<int> GetPaymentMethodId(CreateCreditNoteAsyncCommand command)
        {
            var result = await _commandProcessor.Execute(new GetOrCreatePaymentMethodAsyncCommand(command.FacturalusaService)
            {
                PaymentMethodName = command.Data.PaymentMethodCode,
            });
            return result.Id;
        }

        private async Task<int> GetPaymentConditionId(CreateCreditNoteAsyncCommand command)
        {
            var result = await _commandProcessor.Execute(new GetOrCreatePaymentConditionAsyncCommand(command.FacturalusaService)
            {
                Name = "Pronto pagamento",
                Days = 0,
            });
            return result.Id;
        }

        private async Task<ReadOnlyCustomer> GetCustomer(CreateCreditNoteAsyncCommand command)
        {
            var customer = command.Data.Customer;

            var result = await _commandProcessor.Execute(new GetOrCreateCustomerAsyncCommand(command.FacturalusaService)
            {
                IsFinalConsumer = customer.IsFinalConsumer,
                Type = customer.Type,
                Code = customer.Code,
                Name = customer.Name,
                VatNumber = customer.VatNumber,
                Email = customer.Email,
                MobileNumber = customer.MobileNumber,
                Address =  customer.Address,
                PostalCode = customer.PostalCode,
                CityName = customer.CityName,
                CountryName = customer.CountryName,
            });
            return result;
        }

        private async Task<long> GetSerieId(CreateCreditNoteAsyncCommand command)
        {
            var result = await _commandProcessor.Execute(new GetOrCreateSerieAsyncCommand(command.FacturalusaService)
            {
                SerieName = command.Data.SerieCode,
                ExpirationYear = 2100,
                CommunicateIfNew = settings.CommunicateSeries,
            });
            return result.Id;
        }

        private async Task<IDictionary<ItemData, ReadonlyItem>> GetItems(CreateCreditNoteAsyncCommand command)
        {
            return await _commandProcessor.Execute(new GetOrCreateItemsAsyncCommand(command.FacturalusaService)
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

        private async Task<IEnumerable<VatRate>> GetVatRates(CreateCreditNoteAsyncCommand command)
        {
            return await _commandProcessor.Execute(new GetOrCreateVatRatesAsyncCommand(command.FacturalusaService)
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