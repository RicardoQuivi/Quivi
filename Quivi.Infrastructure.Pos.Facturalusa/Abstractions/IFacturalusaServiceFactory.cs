namespace Quivi.Infrastructure.Pos.Facturalusa.Abstractions
{
    public interface IFacturalusaServiceFactory
    {
        /// <summary>
        /// Create a service to communicate with Facturalusa API.
        /// </summary>
        /// <param name="accessToken">The merchant access token.</param>
        /// <param name="accountUuid">The unique id of facturalusa account. This ID must be different between different accounts.</param>
        /// <returns>The Facturalusa service.</returns>
        IFacturalusaService Create(string accessToken, string accountUuid);
    }
}