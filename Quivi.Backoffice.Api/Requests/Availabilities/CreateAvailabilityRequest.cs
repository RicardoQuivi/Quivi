using Quivi.Backoffice.Api.Dtos;

namespace Quivi.Backoffice.Api.Requests.Availabilities
{
    public class CreateAvailabilityRequest : ARequest
    {
        public required string Name { get; init; } = string.Empty;
        public bool AutoAddNewMenuItems { get; set; }
        public bool AutoAddNewChannelProfiles { get; set; }
        public IEnumerable<WeeklyAvailability>? WeeklyAvailabilities { get; init; }
    }
}