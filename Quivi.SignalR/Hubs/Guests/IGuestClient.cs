using Quivi.SignalR.Dtos;

namespace Quivi.SignalR.Hubs.Guests
{
    public interface IGuestClient
    {
        Task OnSessionUpdated(OnSessionUpdated evt);
        Task OnAcquirerConfigurationOperation(OnAcquirerConfigurationOperation evt);
        Task OnOrderOperation(OnOrderOperation evt);
        Task OnPosChargeOperation(OnPosChargeOperation evt);
    }
}