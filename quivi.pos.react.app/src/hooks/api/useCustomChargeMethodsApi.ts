import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetCustomChargeMethodsResponse } from "./Dtos/customchargemethods/GetCustomChargeMethodsResponse";
import { GetCustomChargeMethodsRequest } from "./Dtos/customchargemethods/GetCustomChargeMethodsRequest";

export const useCustomChargeMethodsApi = (token: string) => {
    const httpClient = useHttpClient();

    const get = async (request: GetCustomChargeMethodsRequest) => {
        const queryParams = new URLSearchParams();
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        request.ids?.forEach((id, i) => queryParams.set(`ids[${i}]`, id));

        const url = new URL(`api/customchargemethods?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpGet<GetCustomChargeMethodsResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient, token]);

    return state;
}