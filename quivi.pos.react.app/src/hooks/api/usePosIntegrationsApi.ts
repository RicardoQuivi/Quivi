import { useMemo } from "react";
import { GetPosIntegrationsRequest } from "./Dtos/posintegrations/GetPosIntegrationsRequest";
import { GetPosIntegrationsResponse } from "./Dtos/posintegrations/GetPosIntegrationsResponse";
import { useEmployeeHttpClient } from "../../context/employee/EmployeeContextProvider";

export const usePosIntegrationsApi = () => {
    const httpClient = useEmployeeHttpClient();

    const get = async (request: GetPosIntegrationsRequest) => {
        const queryParams = new URLSearchParams();
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        queryParams.set("page", request.page.toString());
        
        request.ids?.filter(id => !!id).forEach((id, i) => queryParams.set(`ids[${i}]`, id));

        const url = new URL(`api/posintegrations?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetPosIntegrationsResponse>(url, {});
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient]);

    return state;
}