import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetPosIntegrationsRequest } from "./Dtos/posIntegrations/GetPosIntegrationsRequest";
import { GetPosIntegrationsResponse } from "./Dtos/posIntegrations/GetPosIntegrationsResponse";

export const usePosIntegrationsApi = (token?: string) => {
    const httpClient = useHttpClient();
    
    const get = (request: GetPosIntegrationsRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        if(request.ids != undefined) {
            for(let i = 0; i < request.ids.length; ++i) {
                queryParams.set(`ids[${i}]`, request.ids[i].toString());
            }
        }
        
        if(request.channelId != undefined) {
            queryParams.set("channelId", request.channelId);
        }

        const url = new URL(`api/posintegrations?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpGet<GetPosIntegrationsResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient, token]);

    return state;
}