using Quivi.SignalR.Dtos;

namespace Quivi.SignalR.Hubs.Pos
{
    public interface IPosClient
    {
        Task OnChannelOperation(OnChannelOperation evt);
        Task OnChannelProfileOperation(OnChannelProfileOperation evt);
        Task OnItemCategoryOperation(OnItemCategoryOperation evt);
        Task OnLocationOperation(OnLocationOperation evt);
        Task OnMenuItemOperation(OnMenuItemOperation evt);
        Task OnEmployeeOperation(OnEmployeeOperation evt);
        Task OnItemsModifierGroupOperation(OnItemsModifierGroupOperation evt);
        Task OnCustomChargeMethodOperation(OnCustomChargeMethodOperation evt);
        Task OnSessionUpdated(OnSessionUpdated evt);
        Task OnBackgroundJobUpdated(OnBackgroundJobUpdated evt);
        Task OnPosChargeOperation(OnPosChargeOperation evt);
        Task OnPosChargeSyncAttemptOperation(OnPosChargeSyncAttemptOperation evt);
        Task OnOrderOperation(OnOrderOperation evt);
        Task OnPreparationGroupOperation(OnPreparationGroupOperation evt);
    }
}