namespace Paybyrd.Api.Models
{
    public class ThreeDSecure
    {
        public required string Id { get; init; }
        public required string VerificationMethod { get; init; }
        public required string Channel { get; init; }
        public required string Status { get; init; }
    }
}