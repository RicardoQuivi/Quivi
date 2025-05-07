namespace Quivi.Pos.Api.Dtos.Requests.Transactions
{
    public class CreateTransactionRequest : ARequest
    {
        public required string ChannelId { get; init; }
        public required string CustomChargeMethodId { get; init; }
        public string? LocationId { get; init; }
        public string? Email { get; init; }
        public string? VatNumber { get; init; }
        public string? Observations { get; init; }
        public decimal Total { get; init; }
        public decimal Tip { get; init; }
        public IEnumerable<SessionItem>? Items { get; set; }
    }
}