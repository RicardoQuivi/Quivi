namespace ComplyPay.Dtos.Responses
{
    public class GetVendorAccountsResponse
    {
        public required string StatusMessage { get; init; }
        public int Size { get; init; }
        public required IEnumerable<VendorAccount> VendorAccounts { get; init; }
    }
}