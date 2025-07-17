import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetPosIntegrationsRequest } from "./Dtos/posintegrations/GetPosIntegrationsRequest";
import { GetPosIntegrationsResponse } from "./Dtos/posintegrations/GetPosIntegrationsResponse";

export const usePosIntegrationsApi = (token: string) => {
    const httpClient = useHttpClient();

    const get = async (request: GetPosIntegrationsRequest) => {
        const queryParams = new URLSearchParams();
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        queryParams.set("page", request.page.toString());
        
        request.ids?.filter(id => !!id).forEach((id, i) => queryParams.set(`ids[${i}]`, id));

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