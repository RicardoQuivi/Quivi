namespace Quivi.Backoffice.Api.Requests
{
    public interface IPagedRequest : IRequest
    {
        int Page { get; }
        int? PageSize { get; }
    }
}
