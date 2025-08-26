using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class AssignedOnMapperHandler : IMapperHandler<AssignedOn, Dtos.AssignedOn>,
                                                IMapperHandler<Dtos.AssignedOn, AssignedOn>
    {
        public Dtos.AssignedOn Map(AssignedOn model) => (Dtos.AssignedOn)model;
        public AssignedOn Map(Dtos.AssignedOn model) => (AssignedOn)model;
    }
}