using Quivi.SignalR.Dtos;

namespace Quivi.SignalR.Hubs.Backoffice
{
    public interface IBackofficeClient
    {
        Task OnMerchantAssociated(OnMerchantAssociatedOperation evt);
        Task OnChannelProfileOperation(OnChannelProfileOperation evt);
        Task OnLocationOperation(OnLocationOperation evt);
        Task OnChannelOperation(OnChannelOperation evt);
        Task OnItemCategoryOperation(OnItemCategoryOperation evt);
        Task OnMenuItemOperation(OnMenuItemOperation evt);
        Task OnEmployeeOperation(OnEmployeeOperation evt);
        Task OnItemsModifierGroupOperation(OnItemsModifierGroupOperation evt);
        Task OnCustomChargeMethodOperation(OnCustomChargeMethodOperation evt);
        Task OnPosChargeOperation(OnPosChargeOperation evt);
        Task OnPosChargeSyncAttemptOperation(OnPosChargeSyncAttemptOperation evt);
    }
}