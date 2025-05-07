using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;
using Quivi.Infrastructure.Pos.Facturalusa.Exceptions;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Units;

namespace Quivi.Infrastructure.Pos.Facturalusa.Queries
{
    public class HealthCheckQuery : AFacturalusaAsyncQuery<bool>
    {
        public HealthCheckQuery(IFacturalusaService facturalusaService) : base(facturalusaService)
        {
        }
    }

    public class HealthCheckQueryHandler : IQueryHandler<HealthCheckQuery, Task<bool>>
    {
        public async Task<bool> Handle(HealthCheckQuery query)
        {
            try
            {
                await query.FacturalusaService.GetUnits(new GetUnitsRequest
                {
                    Name = "Unidades",
                });
                return true;
            }
            catch (FacturalusaApiException ex) when (ex.ErrorType == Models.ErrorType.Unauthorized)
            {
                return false;
            }
        }
    }
}
