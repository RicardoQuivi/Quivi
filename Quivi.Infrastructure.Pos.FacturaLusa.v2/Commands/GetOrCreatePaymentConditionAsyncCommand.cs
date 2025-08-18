using FacturaLusa.v2.Dtos;
using FacturaLusa.v2.Dtos.Requests.PaymentConditions;
using FacturaLusa.v2.Exceptions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Commands
{
    public class GetOrCreatePaymentConditionAsyncCommand : AFacturaLusaAsyncCommand<PaymentCondition>
    {
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
        private readonly IFacturaLusaCacheProvider cacheProvider;

        public GetOrCreatePaymentConditionAsyncCommandHandler(IFacturaLusaCacheProvider cacheProvider)
        {
            this.cacheProvider = cacheProvider;
        }

        public async Task<PaymentCondition> Handle(GetOrCreatePaymentConditionAsyncCommand command)
        {
            return await cacheProvider.GetOrCreatePaymentCondition
            (
                command.Service.AccountUuid,
                command.Name,
                () => EntityFactory(command),
                TimeSpan.FromDays(7)
            );
        }

        private async Task<PaymentCondition> EntityFactory(GetOrCreatePaymentConditionAsyncCommand command)
        {
            try
            {
                var response = await command.Service.CreatePaymentCondition(new CreatePaymentConditionRequest
                {
                    Description = command.Name,
                    Days = command.Days,
                });
                return response;
            }
            catch (FacturaLusaException ex) when (ex.ErrorType == ErrorType.FieldValueAlreadyExists)
            {
                var existingResponse = await command.Service.SearchPaymentCondition(new SearchPaymentConditionRequest
                {
                    Value = command.Name,
                });
                return existingResponse;
            }
        }
    }
}
