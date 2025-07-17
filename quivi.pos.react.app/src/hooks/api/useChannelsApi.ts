import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetChannelsRequest } from "./Dtos/channels/GetChannelsRequest";
import { GetChannelsResponse } from "./Dtos/channels/GetChannelsResponse";

export const useChannelsApi = (token: string) => {
    const httpClient = useHttpClient();

    const get = async (request: GetChannelsRequest) => {
        const queryParams = new URLSearchParams();
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        queryParams.set("page", request.page.toString());

        if (!!request.search) {
            queryParams.set("search", request.search);
        }

        if (request.allowsSessionsOnly == true) {
            queryParams.set("allowsSessionsOnly", "true");
        }

        if (request.allowsPostPaidOrderingOnly == true) {
            queryParams.set("allowsPostPaidOrderingOnly", "true");
        }

        if (request.allowsPrePaidOrderingOnly == true) {
            queryParams.set("allowsPrePaidOrderingOnly", "true");
        }

        if (request.hasOpenSession != undefined) {
            queryParams.set("hasOpenSession", request.hasOpenSession == true ? "true" : "false");
        }
        
        if (!!request.channelProfileId) {
            queryParams.set("channelProfileId", request.channelProfileId);
        }

        request.ids?.filter(id => !!id).forEach((id, i) => queryParams.set(`ids[${i}]`, id));
        request.sessionIds?.filter(id => !!id).forEach((id, i) => queryParams.set(`sessionIds[${i}]`, id));

        if (request.includeDeleted == true) {
            queryParams.set("includeDeleted", "true");
        }

        const url = new URL(`api/channels?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpGet<GetChannelsResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient, token]);

    return state;
}