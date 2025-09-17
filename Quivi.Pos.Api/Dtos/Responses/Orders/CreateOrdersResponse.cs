namespace Quivi.Pos.Api.Dtos.Responses.Orders
{
    public class CreateOrdersResponse : AListResponse<Order>
    {
        public string? JobId { get; init; }
    }
}