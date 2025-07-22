namespace Quivi.Pos.Api.Dtos.Requests.Pos
{
    public class PrintEndOfDayClosingRequest : ARequest
    {
        public string? LocationId { get; init; }
    }
}