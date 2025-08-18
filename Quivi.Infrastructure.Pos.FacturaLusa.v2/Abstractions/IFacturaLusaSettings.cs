namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions
{
    public interface IFacturaLusaSettings
    {
        public string Host { get; }
        public string AccessToken { get; }
        public bool CommunicateSeries { get; }
    }
}
