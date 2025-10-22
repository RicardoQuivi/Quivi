using Quivi.Domain.Entities.Financing;
using Quivi.Domain.Entities.Identity;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.People
{
    public class GetPeopleAsyncQuery : APagedAsyncQuery<Person>
    {
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? ChannelIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<string>? Emails { get; init; }
        public IEnumerable<BasicAuthClientType>? ClientTypes { get; init; }
        public IEnumerable<PersonType>? PersonTypes { get; init; }
        public bool? IsAnonymous { get; init; }

        public bool IncludePostings { get; init; }
    }

    public class GetPeopleAsyncQueryHandler : APagedQueryAsyncHandler<GetPeopleAsyncQuery, Person>
    {
        private readonly IPeopleRepository repository;

        public GetPeopleAsyncQueryHandler(IPeopleRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<Person>> Handle(GetPeopleAsyncQuery query)
        {
            return repository.GetAsync(new GetPeopleCriteria
            {
                ParentMerchantIds = query.ParentMerchantIds,
                MerchantIds = query.MerchantIds,
                PersonTypes = query.PersonTypes,
                ChannelIds = query.ChannelIds,
                Emails = query.Emails,
                Ids = query.Ids,
                IsAnonymous = query.IsAnonymous,
                ClientTypes = query.ClientTypes,

                IncludePostings = query.IncludePostings,

                PageSize = query.PageSize,
                PageIndex = query.PageIndex,
            });
        }
    }
}