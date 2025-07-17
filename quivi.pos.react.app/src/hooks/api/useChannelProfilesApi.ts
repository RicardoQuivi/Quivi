import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetChannelProfilesRequest } from "./Dtos/channelProfiles/GetChannelProfilesRequest";
import { GetChannelProfilesResponse } from "./Dtos/channelProfiles/GetChannelProfilesResponse";

export const useChannelProfilesApi = (token: string) => {
    const httpClient = useHttpClient();

    const get = async (request: GetChannelProfilesRequest) => {
        const queryParams = new URLSearchParams();
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        queryParams.set("page", request.page.toString());
        request.ids?.filter(id => !!id).forEach((id, i) => queryParams.set(`ids[${i}]`, id));

        const url = new URL(`api/channelprofiles?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpGet<GetChannelProfilesResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient, token]);

    return state;
}