using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;
using Quivi.Infrastructure.Pos.Facturalusa.Exceptions;
using Quivi.Infrastructure.Pos.Facturalusa.Models.PaymentConditions;

namespace Quivi.Infrastructure.Pos.Facturalusa.Commands
{
    public class GetOrCreatePaymentConditionAsyncCommand : AFacturalusaAsyncCommand<PaymentCondition>
    {
        public GetOrCreatePaymentConditionAsyncCommand(IFacturalusaService facturalusaService) : base(facturalusaService)
        {
        }

        private string _name = string.Empty;
        public required string Name 
        {
            get => _name; 
            set => _name = value.Trim();
        }

        public int Days { get; set; }
    }

    public class GetOrCreatePaymentConditionAsyncCommandHandler : ICommandHandler<GetOrCreatePaymentConditionAsyncCommand, Task<PaymentCondition>>
    {
        private readonly IFacturalusaCacheProvider cacheProvider;

        public GetOrCreatePaymentConditionAsyncCommandHandler(IFacturalusaCacheProvider cacheProvider)
        {
            this.cacheProvider = cacheProvider;
        }

        public async Task<PaymentCondition> Handle(GetOrCreatePaymentConditionAsyncCommand command)
        {
            return await cacheProvider.GetOrCreatePaymentCondition
            (
                command.FacturalusaService.AccountUuid,
                command.Name,
                () => EntityFactory(command),
                TimeSpan.FromDays(7)
            );
        }

        private async Task<PaymentCondition> EntityFactory(GetOrCreatePaymentConditionAsyncCommand command)
        {
            try
            {
                var response = await command.FacturalusaService.CreatePaymentCondition(new CreatePaymentConditionRequest
                {
                    Name = command.Name,
                    Days = command.Days,
                    IsActive = true,
                });
                return response.Data;
            }
            catch (FacturalusaApiException ex) when (ex.ErrorType == Models.ErrorType.FieldValueAlreadyExists)
            {
                var existingResponse = await command.FacturalusaService.GetPaymentConditions(new GetPaymentConditionsRequest
                {
                    Value = command.Name,
                });
                return existingResponse.Data.First(p => string.Equals(p.Name, command.Name, System.StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}