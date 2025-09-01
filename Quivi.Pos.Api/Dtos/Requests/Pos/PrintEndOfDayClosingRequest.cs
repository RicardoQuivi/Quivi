namespace Quivi.Pos.Api.Dtos.Requests.Pos
{
    public class PrintEndOfDayClosingRequest : ARequest
    {
        public string? LocationId { get; init; }

        public required string TitleLabel { get; init; }
        public required string PrintedByLabel { get; init; }
        public required string LocationLabel { get; init; }
        public required string AllLocationsLabel { get; init; }
        public required string TotalLabel { get; init; }
        public required string AmountLabel { get; init; }
        public required string TipsLabel { get; init; }
    }
}