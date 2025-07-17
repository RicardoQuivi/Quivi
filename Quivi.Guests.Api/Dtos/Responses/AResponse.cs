namespace Quivi.Guests.Api.Dtos.Responses
{
    public abstract class AResponse : IResponse
    {
    }

    public abstract class AResponse<T> : AResponse, IResponse<T>
    {
        public required T Data { get; init; }
    }

    public abstract class AListResponse<T> : AResponse<IEnumerable<T>>, IListResponse<T>
    {
    }
}