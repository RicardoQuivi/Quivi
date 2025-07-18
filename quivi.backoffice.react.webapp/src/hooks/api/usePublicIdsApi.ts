import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetPublicIdResponse } from "./Dtos/publicids/GetPublicIdResponse";

export const usePublicIdsApi = (token?: string) => {
    const httpClient = useHttpClient();
 
    const state = useMemo(() => ({
        get: (id: string) => {            
            const url = new URL(`api/publicIds/${id}`, import.meta.env.VITE_API_URL).toString();
            return httpClient.httpGet<GetPublicIdResponse>(url, {
                "Authorization": `Bearer ${token}`,
            });
        },
    }), [httpClient, token]);

    return state;
}