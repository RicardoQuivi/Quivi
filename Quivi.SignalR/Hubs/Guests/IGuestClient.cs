﻿using Quivi.SignalR.Dtos;
using Quivi.SignalR.Dtos.Guests;

namespace Quivi.SignalR.Hubs.Guests
{
    public interface IGuestClient
    {
        Task OnSessionUpdated(OnSessionUpdated evt);
        Task OnAcquirerConfigurationOperation(OnAcquirerConfigurationOperation evt);
        Task OnOrderOperation(OnOrderOperation evt);
        Task OnPosChargeOperation(OnPosChargeOperation evt);
        Task OnBackgroundJobUpdated(OnBackgroundJobUpdated evt);
        Task OnTransactionInvoiceOperation(OnTransactionInvoiceOperation evt);
    }
}