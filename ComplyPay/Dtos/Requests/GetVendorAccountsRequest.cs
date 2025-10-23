namespace ComplyPay.Dtos.Requests
{
    public class GetVendorAccountsRequest
    {
        public IEnumerable<string>? Status { get; init; }
        public IEnumerable<string>? BusinessRegistrations { get; init; }
        public IEnumerable<string>? VATs { get; init; }
        public required IEnumerable<string> ReferenceIds { get; init; }

        public int Page { get; init; }
        public int PageSize { get; init; }
    }
}