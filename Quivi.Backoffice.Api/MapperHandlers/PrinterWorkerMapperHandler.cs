using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class PrinterWorkerMapperHandler : IMapperHandler<PrinterWorker, Dtos.PrinterWorker>
    {
        private readonly IIdConverter idConverter;

        public PrinterWorkerMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public Dtos.PrinterWorker Map(PrinterWorker model)
        {
            return new Dtos.PrinterWorker
            {
                Id = idConverter.ToPublicId(model.Id),
                Identifier = model.Identifier,
                Name = model.Name,
            };
        }
    }
}