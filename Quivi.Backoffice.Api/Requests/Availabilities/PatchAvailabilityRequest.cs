using Quivi.Backoffice.Api.Dtos;

namespace Quivi.Backoffice.Api.Requests.Availabilities
{
    public class PatchAvailabilityRequest : ARequest
    {
        public string? Name { get; init; }
        public bool? AutoAddNewMenuItems { get; set; }
        public bool? AutoAddNewChannelProfiles { get; set; }
        public IEnumerable<WeeklyAvailability>? WeeklyAvailabilities { get; init; }
    }
}