namespace Quivi.Infrastructure.Jobs.Hangfire.Context
{
    public interface IJobContext
    {
        int? MerchantId { get; }
        int? PosIntegrationId { get; }
    }

    public interface IJobContextualizer
    {
        int? MerchantId { set; }
        int? PosIntegrationId { set; }
    }
}