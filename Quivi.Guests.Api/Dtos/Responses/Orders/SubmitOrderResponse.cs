namespace Quivi.Guests.Api.Dtos.Responses.Orders
{
    public class SubmitOrderResponse : AResponse<Order?>
    {
        public required string JobId { get; init; }
    }
}