import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetAcquirerConfigurationsRequest } from "./Dtos/acquirerconfigurations/GetAcquirerConfigurationsRequest";
import { GetAcquirerConfigurationsResponse } from "./Dtos/acquirerconfigurations/GetAcquirerConfigurationsResponse";
import { UpsertAcquirerConfigurationResponse } from "./Dtos/acquirerconfigurations/UpsertAcquirerConfigurationResponse";
import { UpsertCashAcquirerConfigurationRequest } from "./Dtos/acquirerconfigurations/UpsertCashAcquirerConfigurationRequest";

export const useAcquirerConfigurationsApi = (token?: string) => {
    const httpClient = useHttpClient();
 
    const get = (request: GetAcquirerConfigurationsRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        const url = new URL(`api/acquirerConfigurations?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpGet<GetAcquirerConfigurationsResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const upsertCash = (request: UpsertCashAcquirerConfigurationRequest) => {
        const url = new URL(`api/acquirerConfigurations/cash`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpPut<UpsertAcquirerConfigurationResponse>(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        get,
        upsertCash,
    }), [httpClient, token]);

    return state;
}