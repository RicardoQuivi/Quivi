using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.Reviews
{
    public class GetReviewsAsyncQuery : APagedAsyncQuery<Review>
    {
        public IEnumerable<int>? PosChargeIds { get; init; }
    }

    public class GetReviewsAsyncQueryHandler : IQueryHandler<GetReviewsAsyncQuery, Task<IPagedData<Review>>>
    {
        private readonly IReviewsRepository repository;

        public GetReviewsAsyncQueryHandler(IReviewsRepository repository)
        {
            this.repository = repository;
        }

        public Task<IPagedData<Review>> Handle(GetReviewsAsyncQuery query)
        {
            return repository.GetAsync(new GetReviewsCriteria
            {
                PosChargeIds = query.PosChargeIds,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}