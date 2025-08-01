namespace Paybyrd.Api.Models
{
    public class CardDetails
    {
        public required string Number { get; init; }
        public required string Expiration { get; init; }
        public required string Cvv { get; init; }
        public required string Holder { get; init; }
        public int Installments { get; init; }
        public int InstallementAmount { get; init; }
        public bool IsPayerTraveling { get; init; }
        public required string Scheme { get; init; }
        public required string Usage { get; init; }
        public required string CountryCode { get; init; }
    }
}