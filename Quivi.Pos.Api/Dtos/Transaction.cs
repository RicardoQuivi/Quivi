namespace Quivi.Pos.Api.Dtos
{
    public class Transaction
    {
        public required string Id { get; init; }
        public required string ChannelId { get; init; }
        public string? SessionId { get; init; }
        public string? EmployeeId { get; init; }
        public string? RefundEmployeeId { get; init; }
        public string? CustomChargeMethodId { get; init; }
        public decimal Payment { get; init; }
        public decimal Tip { get; init; }
        public decimal RefundedAmount { get; init; }
        public bool IsSynced { get; init; }
        public bool IsFreePayment { get; init; }
        public string? Email { get; init; }
        public string? VatNumber { get; init; }

        public DateTimeOffset CapturedDate { get; init; }
        public DateTimeOffset LastModified { get; init; }

    }
}