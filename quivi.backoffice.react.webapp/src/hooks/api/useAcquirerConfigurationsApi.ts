import { useMemo } from "react";
import { GetAcquirerConfigurationsRequest } from "./Dtos/acquirerconfigurations/GetAcquirerConfigurationsRequest";
import { GetAcquirerConfigurationsResponse } from "./Dtos/acquirerconfigurations/GetAcquirerConfigurationsResponse";
import { UpsertAcquirerConfigurationResponse } from "./Dtos/acquirerconfigurations/UpsertAcquirerConfigurationResponse";
import { UpsertCashAcquirerConfigurationRequest } from "./Dtos/acquirerconfigurations/UpsertCashAcquirerConfigurationRequest";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";
import { UpsertPaybyrdAcquirerConfigurationRequest } from "./Dtos/acquirerconfigurations/UpsertPaybyrdAcquirerConfigurationRequest";
import { ChargeMethod } from "./Dtos/ChargeMethod";

export const useAcquirerConfigurationsApi = () => {
    const client = useAuthenticatedHttpClient();

    const get = (request: GetAcquirerConfigurationsRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        const url = new URL(`api/acquirerConfigurations?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetAcquirerConfigurationsResponse>(url, {});
    }

    const upsertCash = (request: UpsertCashAcquirerConfigurationRequest) => {
        const url = new URL(`api/acquirerConfigurations/cash`, import.meta.env.VITE_API_URL).toString();
        return client.put<UpsertAcquirerConfigurationResponse>(url, request, {});
    }

    const upsertPaybyrd = (request: UpsertPaybyrdAcquirerConfigurationRequest) => {
        const url = new URL(`api/acquirerConfigurations/paybyrd/${ChargeMethod[request.method]}`, import.meta.env.VITE_API_URL).toString();
        return client.put<UpsertAcquirerConfigurationResponse>(url, request, {});
    }

    const state = useMemo(() => ({
        get,
        upsertCash,
        upsertPaybyrd,
    }), [client]);

    return state;
}