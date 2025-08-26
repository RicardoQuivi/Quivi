import { useMemo } from "react";
import { useEmployeeHttpClient } from "../../context/employee/EmployeeContextProvider";
import { GetConfigurableFieldsRequest } from "./Dtos/configurablefields/GetConfigurableFieldsRequest";
import { GetConfigurableFieldsResponse } from "./Dtos/configurablefields/GetConfigurableFieldsResponse";

export const useConfigurableFieldsApi = () => {
    const httpClient = useEmployeeHttpClient();

    const get = async (request: GetConfigurableFieldsRequest) => {
        const queryParams = new URLSearchParams();
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        queryParams.set("page", request.page.toString());
        if(request.forPosSessions != undefined) {
            queryParams.set("forPosSessions", request.forPosSessions ? "true" : "false");
        }

        request.ids?.filter(id => !!id).forEach((id, i) => queryParams.set(`ids[${i}]`, id));

        const url = new URL(`api/configurableFields?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetConfigurableFieldsResponse>(url, {});
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient]);

    return state;
}