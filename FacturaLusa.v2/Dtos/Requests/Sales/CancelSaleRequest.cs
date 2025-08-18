namespace FacturaLusa.v2.Dtos.Requests.Sales
{
    public class CancelSaleRequest
    {
        public required long SaleId { get; init; }
        public required string Reason { get; init; }
    }
}