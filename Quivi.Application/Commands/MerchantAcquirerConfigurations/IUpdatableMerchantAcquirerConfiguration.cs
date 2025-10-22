using Quivi.Domain.Entities.Charges;

namespace Quivi.Application.Commands.MerchantAcquirerConfigurations
{
    public interface IUpdatableMerchantAcquirerConfiguration : IUpdatableEntity
    {
        int MerchantId { get; }
        ChargePartner ChargePartner { get; }
        ChargeMethod ChargeMethod { get; }

        public string? ApiKey { get; set; }
        public string? EntityId { get; set; }
        public string? TerminalId { get; set; }
        public string? WebhookSecret { get; set; }
        public string? PublicKey { get; set; }
        public bool ExternallySettled { get; set; }

        public bool Inactive { get; set; }
    }
}