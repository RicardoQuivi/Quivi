namespace Quivi.Pos.Api.Dtos.Requests
{
    public interface IPagedRequest : IRequest
    {
        int Page { get; }
        int? PageSize { get; }
    }
}
