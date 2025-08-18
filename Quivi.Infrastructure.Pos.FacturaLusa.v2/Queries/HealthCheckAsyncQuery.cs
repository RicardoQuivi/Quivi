using FacturaLusa.v2.Exceptions;
using Quivi.Infrastructure.Abstractions.Cqrs;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Queries
{
    public class HealthCheckAsyncQuery : AFacturaLusaAsyncQuery<bool>
    {
    }

    public class HealthCheckAsyncQueryHandler : IQueryHandler<HealthCheckAsyncQuery, Task<bool>>
    {
        public async Task<bool> Handle(HealthCheckAsyncQuery query)
        {
            try
            {
                await query.Service.SearchUnit(new global::FacturaLusa.v2.Dtos.Requests.Units.SearchUnitRequest
                {
                    Value = "Unidades",
                });
                return true;
            }
            catch (NotAuthorizedException)
            {
                return false;
            }
        }
    }
}