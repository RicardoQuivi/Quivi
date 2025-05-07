using Quivi.Domain.Repositories.EntityFramework.Identity;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.Users
{
    public class GetApplicationUsersAsyncQuery : APagedAsyncQuery<ApplicationUser>
    {
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public bool IncludeMerchants { get; set; }
    }

    public class GetApplicationUsersAsyncQueryHandler : IQueryHandler<GetApplicationUsersAsyncQuery, Task<IPagedData<ApplicationUser>>>
    {
        private readonly IApplicationUsersRepository repository;

        public GetApplicationUsersAsyncQueryHandler(IApplicationUsersRepository repository)
        {
            this.repository = repository;
        }

        public Task<IPagedData<ApplicationUser>> Handle(GetApplicationUsersAsyncQuery query)
        {
            return repository.GetAsync(new GetApplicationUsersCriteria
            {
                Ids = query.Ids,
                MerchantIds = query.MerchantIds,
                IncludeMerchants = query.IncludeMerchants,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}
