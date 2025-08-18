using FacturaLusa.v2;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2
{
    public class FacturaLusaServiceFactory : IFacturaLusaServiceFactory
    {
        private readonly IFacturaLusaApi api;

        public FacturaLusaServiceFactory(IFacturaLusaApi api)
        {
            this.api = api;
        }

        public IFacturaLusaService Create(string accessToken, string accountUuid) => new FacturaLusaService(accountUuid, accessToken, api);
    }
}
