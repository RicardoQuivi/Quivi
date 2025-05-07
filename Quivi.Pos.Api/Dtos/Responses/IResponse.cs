namespace Quivi.Pos.Api.Dtos.Responses
{
    public interface IResponse
    {
    }

    public interface IResponse<T> : IResponse
    {
        T Data { get; }
    }

    public interface IListResponse<T> : IResponse<IEnumerable<T>>
    {
    }
}
