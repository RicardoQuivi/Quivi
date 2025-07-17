namespace Quivi.Guests.Api.Dtos.Requests
{
    public interface IPagedRequest : IRequest
    {
        int Page { get; }
        int? PageSize { get; }
    }
}