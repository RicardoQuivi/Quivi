using Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions;

namespace Quivi.Application.Configurations
{
    public class FacturaLusaSettings : IFacturaLusaSettings
    {
        public required string Host { get; init; }
        public required string AccessToken { get; init; }
        public bool CommunicateSeries { get; init; }
    }
}
