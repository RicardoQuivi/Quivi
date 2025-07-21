namespace Quivi.Pos.Api.Dtos.Requests.Pos
{
    public class PrintConsumerBillRequest : ARequest
    {
        public string SessionId { get; init; }
        public string? LocationId { get; init; }
    }
}