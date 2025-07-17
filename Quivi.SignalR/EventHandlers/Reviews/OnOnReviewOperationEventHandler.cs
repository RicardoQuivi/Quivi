using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.Reviews;
using Quivi.SignalR.Hubs.Guests;

namespace Quivi.SignalR.EventHandlers.Reviews
{
    public class OnOnReviewOperationEventHandler : IEventHandler<OnReviewOperationEvent>
    {
        private readonly IHubContext<GuestsHub, IGuestClient> guestsHub;
        private readonly IIdConverter idConverter;

        public OnOnReviewOperationEventHandler(IHubContext<GuestsHub, IGuestClient> guestsHub,
                                                IIdConverter idConverter)
        {
            this.guestsHub = guestsHub;
            this.idConverter = idConverter;
        }

        public async Task Process(OnReviewOperationEvent message)
        {

        }
    }
}