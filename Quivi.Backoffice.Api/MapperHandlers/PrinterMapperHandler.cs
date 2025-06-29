using Quivi.Domain.Entities.Notifications;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class PrinterMapperHandler : IMapperHandler<PrinterNotificationsContact, Dtos.Printer>
    {
        private readonly IIdConverter idConverter;
        private readonly IMapper mapper;

        public PrinterMapperHandler(IIdConverter idConverter, IMapper mapper)
        {
            this.idConverter = idConverter;
            this.mapper = mapper;
        }

        public Dtos.Printer Map(PrinterNotificationsContact model)
        {
            return new Dtos.Printer
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.Name,
                Address = model.Address,
                PrinterWorkerId = idConverter.ToPublicId(model.PrinterWorkerId),
                LocationId = model.LocationId.HasValue ? idConverter.ToPublicId(model.LocationId.Value) : null,
                Notifications = mapper.Map<IEnumerable<Dtos.NotificationType>>(model.BaseNotificationsContact!.SubscribedNotifications),
            };
        }
    }
}