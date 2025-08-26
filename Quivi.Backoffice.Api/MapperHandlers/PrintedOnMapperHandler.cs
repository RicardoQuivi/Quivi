using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class PrintedOnMapperHandler : IMapperHandler<PrintedOn, Dtos.PrintedOn>,
                                            IMapperHandler<Dtos.PrintedOn, PrintedOn>
    {
        public Dtos.PrintedOn Map(PrintedOn model) => (Dtos.PrintedOn)model;
        public PrintedOn Map(Dtos.PrintedOn model) => (PrintedOn)model;
    }
}