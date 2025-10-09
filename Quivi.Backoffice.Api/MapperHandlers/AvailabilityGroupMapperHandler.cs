using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class AvailabilityGroupMapperHandler : IMapperHandler<AvailabilityGroup, Dtos.Availability>
    {
        private readonly IIdConverter idConverter;

        private class WeekdayDuration : Dtos.WeeklyAvailability
        {
            public DayOfWeek DayOfWeek { get; init; }
        }

        public AvailabilityGroupMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public Dtos.Availability Map(AvailabilityGroup model)
        {
            return new Dtos.Availability
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.Name,
                AutoAddNewChannelProfiles = model.AutoAddNewChannelProfiles,
                AutoAddNewMenuItems = model.AutoAddNewMenuItems,
                WeeklyAvailabilities = model.WeeklyAvailabilities?.Select(s => new Dtos.WeeklyAvailability
                {
                    StartAt = s.StartAt,
                    EndAt = s.EndAt,
                }) ?? [],
            };
        }
    }
}