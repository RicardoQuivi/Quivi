namespace Quivi.Infrastructure.Abstractions.Pos.Invoicing
{
    public interface IInvoiceGatewayFactory
    {
        IInvoiceGateway GetInvoiceGateway(ISyncSettings settings);
    }
}