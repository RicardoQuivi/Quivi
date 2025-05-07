using Quivi.Domain.Entities.Pos;

namespace Quivi.Backoffice.Api.Requests.ChannelProfiles
{
    public class PatchChannelProfileRequest
    {
        #region SentToPreparationTimer
        private TimeSpan? sendToPreparationTimer;
        public bool SendToPreparationTimerWasSet { get; private set; }
        public TimeSpan? SendToPreparationTimer
        {
            get => sendToPreparationTimer;
            init
            {
                SendToPreparationTimerWasSet = true;
                sendToPreparationTimer = value;
            }
        }
        #endregion

        public required string Name { get; init; }
        public decimal? MinimumPrePaidOrderAmount { get; init; }
        public ChannelFeature? Features { get; init; }
        public required string PosIntegrationId { get; init; }
    }
}
