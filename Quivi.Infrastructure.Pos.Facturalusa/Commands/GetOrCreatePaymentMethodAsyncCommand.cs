using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;
using Quivi.Infrastructure.Pos.Facturalusa.Exceptions;
using Quivi.Infrastructure.Pos.Facturalusa.Models.PaymentMethods;

namespace Quivi.Infrastructure.Pos.Facturalusa.Commands
{
    public class GetOrCreatePaymentMethodAsyncCommand : AFacturalusaAsyncCommand<ReadonlyPaymentMethod>
    {
        public GetOrCreatePaymentMethodAsyncCommand(IFacturalusaService facturalusaService) : base(facturalusaService)
        {
        }

        private string _paymentMethodName = string.Empty;
        public required string PaymentMethodName
        {
            get => _paymentMethodName;
            set => _paymentMethodName = value.Trim();
        }
    }

    public class GetOrCreatePaymentMethodAsyncCommandHandler : ICommandHandler<GetOrCreatePaymentMethodAsyncCommand, Task<ReadonlyPaymentMethod>>
    {
        private readonly IFacturalusaCacheProvider cacheProvider;

        public GetOrCreatePaymentMethodAsyncCommandHandler(IFacturalusaCacheProvider cacheProvider)
        {
            this.cacheProvider = cacheProvider;
        }

        public async Task<ReadonlyPaymentMethod> Handle(GetOrCreatePaymentMethodAsyncCommand command)
        {
            return await cacheProvider.GetOrCreatePaymentMethod
            (
                command.FacturalusaService.AccountUuid,
                command.PaymentMethodName,
                () => EntityFactory(command),
                TimeSpan.FromDays(7)
            );
        }

        private async Task<ReadonlyPaymentMethod> EntityFactory(GetOrCreatePaymentMethodAsyncCommand command)
        {
            try
            {
                var response = await command.FacturalusaService.CreatePaymentMethod(new CreatePaymentMethodRequest
                {
                    Name = command.PaymentMethodName,
                    IsActive = true,
                });
                return response.Data;
            }
            catch (FacturalusaApiException ex) when (ex.ErrorType == Models.ErrorType.FieldValueAlreadyExists)
            {
                var existingResponse = await command.FacturalusaService.GetPaymentMethods(new GetPaymentMethodsRequest
                {
                    Value = command.PaymentMethodName,
                });
                return existingResponse.Data.First(p => string.Equals(p.Name, command.PaymentMethodName, System.StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}
