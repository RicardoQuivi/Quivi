namespace Quivi.Backoffice.Api.Requests
{
    public abstract class APagedRequest : ARequest, IPagedRequest
    {
        public int Page { get; init; } = 0;
        public int? PageSize { get; init; } = null;
    }
}
