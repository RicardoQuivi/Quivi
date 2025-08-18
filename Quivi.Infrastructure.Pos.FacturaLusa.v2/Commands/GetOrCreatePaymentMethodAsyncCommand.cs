using FacturaLusa.v2.Dtos;
using FacturaLusa.v2.Dtos.Requests.PaymentMethods;
using FacturaLusa.v2.Exceptions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Commands
{
    public class GetOrCreatePaymentMethodAsyncCommand : AFacturaLusaAsyncCommand<PaymentMethod>
    {
        private string _paymentMethodName = string.Empty;
        public required string PaymentMethodName
        {
            get => _paymentMethodName;
            set => _paymentMethodName = value.Trim();
        }
    }

    public class GetOrCreatePaymentMethodAsyncCommandHandler : ICommandHandler<GetOrCreatePaymentMethodAsyncCommand, Task<PaymentMethod>>
    {
        private readonly IFacturaLusaCacheProvider cacheProvider;

        public GetOrCreatePaymentMethodAsyncCommandHandler(IFacturaLusaCacheProvider cacheProvider)
        {
            this.cacheProvider = cacheProvider;
        }

        public async Task<PaymentMethod> Handle(GetOrCreatePaymentMethodAsyncCommand command)
        {
            return await cacheProvider.GetOrCreatePaymentMethod
            (
                command.Service.AccountUuid,
                command.PaymentMethodName,
                () => EntityFactory(command),
                TimeSpan.FromDays(7)
            );
        }

        private async Task<PaymentMethod> EntityFactory(GetOrCreatePaymentMethodAsyncCommand command)
        {
            try
            {
                var response = await command.Service.CreatePaymentMethod(new CreatePaymentMethodRequest
                {
                    Description = command.PaymentMethodName,
                });
                return response;
            }
            catch (FacturaLusaException ex) when (ex.ErrorType == ErrorType.FieldValueAlreadyExists)
            {
                var existingResponse = await command.Service.SearchPaymentMethod(new SearchPaymentMethodRequest
                {
                    Value = command.PaymentMethodName,
                });
                return existingResponse;
            }
        }
    }
}
