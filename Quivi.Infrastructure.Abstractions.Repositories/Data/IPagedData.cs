namespace Quivi.Infrastructure.Abstractions.Repositories.Data
{
    public interface IPagedData<T> : IEnumerable<T>
    {
        public int NumberOfPages { get; }
        public int CurrentPage { get; }
        public int TotalItems { get; }
    }
}
