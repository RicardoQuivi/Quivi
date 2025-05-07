import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetSessionsRequest } from "./Dtos/sessions/GetSessionsRequest";
import { GetSessionsResponse } from "./Dtos/sessions/GetSessionsResponse";

export const useSessionsApi = (token: string) => {
    const httpClient = useHttpClient();

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

        let url = `${import.meta.env.VITE_API_URL}api/sessions?${queryParams}`;
        return httpClient.httpGet<GetSessionsResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient, token]);

    return state;
}