namespace Quivi.Backoffice.Api.Dtos
{
    public class Merchant
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string VatNumber { get; init; }
        public required string LogoUrl { get; init; }
        public required decimal TransactionFee { get; init; }
        public required decimal SetUpFee { get; init; }
        public required string? ParentId { get; init; }
    }
}