﻿using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.Employees
{
    public class GetEmployeesAsyncQuery : APagedAsyncQuery<Employee>
    {
        public IEnumerable<int>? MerchantIds { get; set; }
        public IEnumerable<int>? Ids { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class GetEmployeesAsyncQueryHandler : APagedQueryAsyncHandler<GetEmployeesAsyncQuery, Employee>
    {
        private readonly IEmployeesRepository repository;

        public GetEmployeesAsyncQueryHandler(IEmployeesRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<Employee>> Handle(GetEmployeesAsyncQuery query)
        {
            return repository.GetAsync(new GetEmployeesCriteria
            {
                Ids = query.Ids,
                MerchantIds = query.MerchantIds,
                IsDeleted = query.IsDeleted,

                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}
