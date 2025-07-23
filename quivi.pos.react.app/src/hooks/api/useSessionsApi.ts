import { useMemo } from "react";
import { GetSessionsRequest } from "./Dtos/sessions/GetSessionsRequest";
import { GetSessionsResponse } from "./Dtos/sessions/GetSessionsResponse";
import { useEmployeeHttpClient } from "../../context/employee/EmployeeContextProvider";

export const useSessionsApi = () => {
    const httpClient = useEmployeeHttpClient();

    const get = async (request: GetSessionsRequest) => {
        const queryParams = new URLSearchParams();
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        queryParams.set("page", request.page.toString());

        if(request.isOpen != undefined) {
            queryParams.set("isOpen", request.isOpen ? "true" : "false");
        }

        if(request.includeDeleted != undefined) {
            queryParams.set("includeDeleted", request.includeDeleted ? "true" : "false");
        }

        request.ids?.forEach((id, i) => queryParams.set(`ids[${i}]`, id));
        request.channelIds?.forEach((id, i) => queryParams.set(`channelIds[${i}]`, id));

        const url = new URL(`api/sessions?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetSessionsResponse>(url, {});
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient]);

    return state;
}