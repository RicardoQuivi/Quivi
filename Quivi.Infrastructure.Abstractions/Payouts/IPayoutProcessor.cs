namespace Quivi.Infrastructure.Abstractions.Payouts
{
    public interface IPayoutProcessor
    {
        Task Process(IEnumerable<Payout> payouts);
    }
}
