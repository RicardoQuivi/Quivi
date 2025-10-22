using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Merchants;

namespace Quivi.Application.Commands.MerchantAcquirerConfigurations
{
    public class UpdatableMerchantAcquirerConfiguration : IUpdatableMerchantAcquirerConfiguration
    {
        public readonly MerchantAcquirerConfiguration Model;
        private readonly DateTime now;
        private readonly string? originalApiKey;
        private readonly string? originalEntityId;
        private readonly string? originalTerminalId;
        private readonly string? originalWebhookSecret;
        private readonly string? originalPublicKey;
        private readonly bool originalExternallySettled;
        private readonly bool originalDeleted;

        public UpdatableMerchantAcquirerConfiguration(MerchantAcquirerConfiguration model, DateTime now)
        {
            this.Model = model;
            this.now = now;

            this.originalApiKey = model.ApiKey;
            this.originalEntityId = model.EntityId;
            this.originalTerminalId = model.TerminalId;
            this.originalWebhookSecret = model.WebhookSecret;
            this.originalPublicKey = model.PublicKey;
            this.originalExternallySettled = model.ExternallySettled;
            this.originalDeleted = model.DeletedDate.HasValue;
        }

        public int MerchantId => Model.MerchantId;
        public ChargePartner ChargePartner => Model.ChargePartner;
        public ChargeMethod ChargeMethod => Model.ChargeMethod;

        public string? ApiKey
        {
            get => Model.ApiKey;
            set => Model.ApiKey = value;
        }
        public string? EntityId
        {
            get => Model.EntityId;
            set => Model.EntityId = value;
        }
        public string? TerminalId
        {
            get => Model.TerminalId;
            set => Model.TerminalId = value;
        }
        public string? WebhookSecret
        {
            get => Model.WebhookSecret;
            set => Model.WebhookSecret = value;
        }
        public string? PublicKey
        {
            get => Model.PublicKey;
            set => Model.PublicKey = value;
        }
        public bool ExternallySettled
        {
            get => Model.ExternallySettled;
            set => Model.ExternallySettled = value;
        }
        public bool Inactive
        {
            get => Model.DeletedDate.HasValue;
            set => Model.DeletedDate = value ? this.now : null;
        }

        public bool WasDeleted => originalDeleted == false && Model.DeletedDate.HasValue;
        public bool WasRestored => originalDeleted && Model.DeletedDate.HasValue == false;

        public bool HasChanges
        {
            get
            {
                if (ApiKey != originalApiKey)
                    return true;

                if (EntityId != originalEntityId)
                    return true;

                if (TerminalId != originalTerminalId)
                    return true;

                if (WebhookSecret != originalWebhookSecret)
                    return true;

                if (PublicKey != originalPublicKey)
                    return true;

                if (ExternallySettled != originalExternallySettled)
                    return true;

                if (originalDeleted != Model.DeletedDate.HasValue)
                    return true;

                return false;
            }
        }
    }
}