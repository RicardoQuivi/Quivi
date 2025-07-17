using Quivi.Domain.Entities.Charges;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.MapperHandlers
{
    public class TransactionStatusMapperHandler : IMapperHandler<ChargeStatus, Dtos.TransactionStatus>
    {
        public Dtos.TransactionStatus Map(ChargeStatus model)
        {
            return model switch
            {
                ChargeStatus.Expired => Dtos.TransactionStatus.Expired,
                ChargeStatus.Failed => Dtos.TransactionStatus.Failed,
                ChargeStatus.Requested => Dtos.TransactionStatus.Processing,
                ChargeStatus.Processing => Dtos.TransactionStatus.Processing,
                ChargeStatus.Completed => Dtos.TransactionStatus.Success,
                _ => throw new NotImplementedException(),
            };
        }
    }
}