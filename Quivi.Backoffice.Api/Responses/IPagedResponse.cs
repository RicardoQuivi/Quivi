namespace Quivi.Backoffice.Api.Responses
{
    public interface IPagedResponse<T> : IResponse<IEnumerable<T>>
    {
        int Page { get; }
        int TotalPages { get; }
        int TotalItems { get; }
    }
}
