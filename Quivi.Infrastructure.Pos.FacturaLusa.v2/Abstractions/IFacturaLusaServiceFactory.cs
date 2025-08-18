namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions
{
    public interface IFacturaLusaServiceFactory
    {
        IFacturaLusaService Create(string accessToken, string accountUuid);
    }
}