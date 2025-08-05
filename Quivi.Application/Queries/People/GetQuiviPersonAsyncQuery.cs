using Quivi.Domain.Entities.Financing;
using Quivi.Infrastructure.Abstractions.Cqrs;

namespace Quivi.Application.Queries.People
{
    public class GetQuiviPersonAsyncQuery : IQuery<Task<Person>>
    {

    }

    public class GetQuiviPersonAsyncQueryHandler : IQueryHandler<GetQuiviPersonAsyncQuery, Task<Person>>
    {
        private readonly IQueryProcessor queryProcessor;
        public GetQuiviPersonAsyncQueryHandler(IQueryProcessor queryProcessor)
        {
            this.queryProcessor = queryProcessor;
        }
        public async Task<Person> Handle(GetQuiviPersonAsyncQuery query)
        {
            var quiviPersonQuery = await queryProcessor.Execute(new GetPeopleAsyncQuery
            {
                PersonTypes = [PersonType.Quivi],
                PageSize = 1,
            });
            var quiviPerson = quiviPersonQuery.Single();
            return quiviPerson;
        }
    }
}