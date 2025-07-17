using Quivi.Domain.Entities.Charges;

namespace Quivi.Guests.Api.Dtos.Requests.Transactions
{
    public abstract class PutTransactionRequest : ARequest
    {
        public abstract ChargeMethod ChargeMethod { get; }
    }
}