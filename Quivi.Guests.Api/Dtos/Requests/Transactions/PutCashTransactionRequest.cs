using Quivi.Domain.Entities.Charges;

namespace Quivi.Guests.Api.Dtos.Requests.Transactions
{
    public class PutCashTransactionRequest : PutTransactionRequest
    {
        public override ChargeMethod ChargeMethod => ChargeMethod.Cash;
    }
}