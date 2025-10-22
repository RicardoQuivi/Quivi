using Quivi.Infrastructure.Abstractions.Repositories.Data;
using System.Collections;

namespace Quivi.Infrastructure.Repositories
{
    public class PagedData<T> : IPagedData<T>
    {
        private IEnumerable<T> Data { get; }
        public required int NumberOfPages { get; init; }
        public required int CurrentPage { get; init; }
        public required int TotalItems { get; init; }

        public PagedData(IEnumerable<T> Data)
        {
            this.Data = Data;
        }

        public IEnumerator<T> GetEnumerator() => Data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Data.GetEnumerator();
    }
}
