import { useMemo } from "react";
import type { PaymentDetails, PaymentDivionDetails } from "../pages/Pay/paymentTypes";

const saveStorageEntry = (storage: Storage, key: string, value: string | null) => {
    if (value) {
        storage.setItem(key, value);
    } else {
        storage.removeItem(key);
    }
}

const getLocalStorageEntry = (key: string) => localStorage.getItem(key);
const saveLocalStorageEntry = (key: string, value: string | null) => saveStorageEntry(localStorage, key, value);
const getSessionStorageEntry = (key: string) => sessionStorage.getItem(key);
const saveSessionStorageEntry = (key: string, value: string | null) => saveStorageEntry(sessionStorage, key, value);

export const useBrowserStorageService = () => {
    const service = useMemo(() => ({
        getAccessToken: () => getLocalStorageEntry("accessToken"),
        saveAccessToken: (value: string | null) => saveLocalStorageEntry("accessToken", value),

        getRefreshToken: () => getLocalStorageEntry("refreshToken"),
        saveRefreshToken: (value: string | null) => saveLocalStorageEntry("refreshToken", value),

        getOrderId: () => getLocalStorageEntry("orderId"),
        saveOrderId: (value: string | null) => saveLocalStorageEntry("orderId", value),

        getChargeId: () => getLocalStorageEntry("chargeId"),
        saveChargeId: (value: string | null) => saveLocalStorageEntry("chargeId", value),

        getChannelId: () => getSessionStorageEntry("channelId"),
        saveChannelId:  (value: string | null) => saveSessionStorageEntry("channelId", value),

        getPaymentDivision: (): PaymentDivionDetails | null => {
            const r = getSessionStorageEntry("paymentDivision");
            if(r == null) {
                return null;
            }
            return JSON.parse(r);
        },
        savePaymentDivision: (value: PaymentDivionDetails | null) => {
            if(value == null) {
                saveSessionStorageEntry("paymentDivision", null);
            } else {
                saveSessionStorageEntry("paymentDivision", JSON.stringify(value));
            }
        },

        getPaymentDetails: (): PaymentDetails | null => {
            const r = getSessionStorageEntry("paymentDetails");
            if(r == null) {
                return null;
            }
            return JSON.parse(r);
        },
        savePaymentDetails: (value: PaymentDetails | null) => {
            if(value == null) {
                saveSessionStorageEntry("paymentDetails", null);
            } else {
                saveSessionStorageEntry("paymentDetails", JSON.stringify(value));
            }
        },
    }), []);

    return service;
}
