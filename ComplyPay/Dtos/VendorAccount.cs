namespace ComplyPay.Dtos
{
    public class VendorAccount
    {
        public required string AccountIdentifier { get; init; }
        public required string Type { get; init; }
        public int Company { get; init; }
        public required string Description { get; init; }
        public required string PayoutIban { get; init; }
        public required string VirtualIban { get; init; }
        public required IEnumerable<string> Currencies { get; init; }
        public required string ReferenceId { get; init; }
    }
}