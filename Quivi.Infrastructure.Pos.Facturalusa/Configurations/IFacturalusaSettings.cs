namespace Quivi.Infrastructure.Pos.Facturalusa.Configurations
{
    public interface IFacturalusaSettings
    {
        public string Host { get; }
        public string AccessToken { get; }
        public bool CommunicateSeries { get; }
    }
}