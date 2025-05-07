using Quivi.Infrastructure.Abstractions.Services;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;
using Quivi.Infrastructure.Pos.Facturalusa.Configurations;

namespace Quivi.Infrastructure.Pos.Facturalusa
{
    public class FacturalusaServiceFactory : IFacturalusaServiceFactory
    {
        private readonly ILogger logger;
        private readonly IFacturalusaSettings settings;

        public FacturalusaServiceFactory(ILogger logger, IFacturalusaSettings settings)
        {
            this.logger = logger;
            this.settings = settings;
        }

        public IFacturalusaService Create(string accessToken, string accountUuid) => new FacturalusaService(accountUuid, settings.Host, new HttpFacturalusaHandler(logger, accessToken));
    }
}
