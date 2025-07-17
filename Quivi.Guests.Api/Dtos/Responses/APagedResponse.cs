namespace Quivi.Guests.Api.Dtos.Responses
{
    public abstract class APagedResponse<T> : AListResponse<T>, IPagedResponse<T>
    {
        public int Page { get; init; }
        public int TotalPages { get; init; }
        public int TotalItems { get; init; }
    }
}