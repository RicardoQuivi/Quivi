import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import type { GetMerchantResponse } from "./Dtos/merchants/GetMerchantResponse";

export const useMerchantsApi = () => {
    const httpClient = useHttpClient();
    
    const get = (merchantId: string) => {
        const url = new URL(`api/merchants/${merchantId}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetMerchantResponse>(url);
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient]);

    return state;
}