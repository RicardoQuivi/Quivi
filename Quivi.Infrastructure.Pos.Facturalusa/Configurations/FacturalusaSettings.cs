namespace Quivi.Infrastructure.Pos.Facturalusa.Configurations
{
    public class FacturalusaSettings : IFacturalusaSettings
    {
        public required string Host { get; init; }
        public required string AccessToken { get; init; }
        public bool CommunicateSeries { get; init; }
    }
}