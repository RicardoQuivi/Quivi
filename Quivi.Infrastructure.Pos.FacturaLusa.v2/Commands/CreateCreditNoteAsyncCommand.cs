using FacturaLusa.v2.Dtos;
using FacturaLusa.v2.Dtos.Requests.Sales;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Models;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Commands
{
    public class CreateCreditNoteAsyncCommand : AFacturaLusaAsyncCommand<Sale>
    {
        public DocumentFormat? Format { get; set; }
        public required CreateCreditNoteData Data { get; init; }

        public class CreateCreditNoteData
        {
            public required string SerieCode { get; init; }
            public required string RelatedDocumentId { get; init; }
            public required string PaymentMethodCode { get; init; }
            public required DateTime CreatedDateLocal { get; init; }
            public required SaleDocumentCustomer Customer { get; init; }
            public string? Observations { get; init; }
            public VatType VatType { get; init; }
            public required IEnumerable<SaleDocumentItem> Items { get; init; }
        }
    }

    public class CreateCreditNoteAsyncCommandHandler : AGetOrCreateSaleCommandHandler, ICommandHandler<CreateCreditNoteAsyncCommand, Task<Sale>>
    {
        private readonly ICommandProcessor commandProcessor;
        private readonly IQueryProcessor queryProcessor;
        private readonly IFacturaLusaSettings settings;

        public CreateCreditNoteAsyncCommandHandler(
            ICommandProcessor commandProcessor,
            IQueryProcessor queryProcessor,
            IFacturaLusaSettings settings,
            IFacturaLusaCacheProvider cacheProvider) : base(cacheProvider)
        {
            this.commandProcessor = commandProcessor;
            this.queryProcessor = queryProcessor;
            this.settings = settings;
        }

        private string GetItemKey(string reference, string correlationId) => $"{reference}_{correlationId}";

        public async Task<Sale> Handle(CreateCreditNoteAsyncCommand command)
        {
            var items = await GetItems(command);
            var itemsCodeIdRelation = items.ToDictionary(x => GetItemKey(x.Key.Reference, x.Key.CorrelationId), x => x.Value.Id);
            var genericItem = new Lazy<Item>(() => items.Select(x => x.Value).First(x => x.Reference == "Diversos"));

            var vatRates = await GetVatRates(command);
            var vatRatesValueIdRelation = vatRates.GroupBy(v => v.TaxPercentage).ToDictionary(x => x.Key, x => x.First().Id);

            var customer = await GetCustomer(command);
            var currency = await GetCurrency(command);
            var paymentMethod = await GetPaymentMethod(command);
            var paymentCondition = await GetPaymentCondition(command);
            var serie = await GetSerie(command);

            var newSaleResponse = await command.Service.CreateSale(new CreateSaleRequest
            {
                Status = SaleStatus.Final,
                SaleReferenceId = await GetReferenceSaleId(command),
                CurrencyId = currency.Id,
                PaymentMethodId = paymentMethod.Id,
                PaymentConditionId = paymentCondition.Id,
                SerieId = serie.Id,
                IssueDate = command.Data.CreatedDateLocal,
                CustomerId = customer.Id,
                VatNumber = Set(command.Data.Customer.VatNumber, customer.VatNumber!),
                Address = Set(command.Data.Customer.Address, customer.Address!),
                City = Set(command.Data.Customer.City, customer.City!),
                PostalCode = Set(command.Data.Customer.PostalCode, customer.PostalCode!),
                Country = Set(command.Data.Customer.Country, customer.Country!),
                DocumentType = DocumentType.CreditNote,
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
                Format = command.Format,
                ForcePrint = command.Format.HasValue,
            });

            await PostSaleCreation(command.Service, newSaleResponse);

            return newSaleResponse;
        }

        private string Set(string? value, string defaultValue) => string.IsNullOrWhiteSpace(value) ? defaultValue : value;

        private Task<long> GetReferenceSaleId(CreateCreditNoteAsyncCommand command)
        {
            return queryProcessor.Execute(new GetFacturaLusaSaleIdAsyncQuery
            {
                Service = command.Service,
                DocumentId = command.Data.RelatedDocumentId,
            });
        }

        private Task<Currency> GetCurrency(CreateCreditNoteAsyncCommand command)
        {
            return commandProcessor.Execute(new GetOrCreateCurrencyAsyncCommand
            {
                Service = command.Service,
                IsoCode = "EUR",
                Name = "Euro",
                Symbol = "€",
            });
        }

        private Task<PaymentMethod> GetPaymentMethod(CreateCreditNoteAsyncCommand command)
        {
            return commandProcessor.Execute(new GetOrCreatePaymentMethodAsyncCommand
            {
                Service = command.Service,
                PaymentMethodName = command.Data.PaymentMethodCode,
            });
        }

        private Task<PaymentCondition> GetPaymentCondition(CreateCreditNoteAsyncCommand command)
        {
            return commandProcessor.Execute(new GetOrCreatePaymentConditionAsyncCommand
            {
                Service = command.Service,
                Name = "Pronto pagamento",
                Days = 0,
            });
        }

        private async Task<Customer> GetCustomer(CreateCreditNoteAsyncCommand command)
        {
            var customer = command.Data.Customer;

            var result = await commandProcessor.Execute(new GetOrCreateCustomerAsyncCommand
            {
                Service = command.Service,
                IsFinalConsumer = customer.IsFinalConsumer,
                Type = customer.Type,
                Code = customer.Code,
                Name = customer.Name,
                VatNumber = customer.VatNumber,
                Email = customer.Email,
                MobileNumber = customer.MobileNumber,
                Address = customer.Address,
                PostalCode = customer.PostalCode,
                CityName = customer.City,
                CountryName = customer.Country,
            });
            return result;
        }

        private Task<Serie> GetSerie(CreateCreditNoteAsyncCommand command)
        {
            return commandProcessor.Execute(new GetOrCreateSerieAsyncCommand
            {
                Service = command.Service,
                SerieName = command.Data.SerieCode,
                ExpirationYear = 2100,
                CommunicateIfNew = settings.CommunicateSeries,
            });
        }

        private Task<IDictionary<ItemData, Item>> GetItems(CreateCreditNoteAsyncCommand command)
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

        private Task<IEnumerable<VatRate>> GetVatRates(CreateCreditNoteAsyncCommand command)
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
