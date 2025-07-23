import { useMemo } from "react";
import { GetChannelProfilesRequest } from "./Dtos/channelProfiles/GetChannelProfilesRequest";
import { GetChannelProfilesResponse } from "./Dtos/channelProfiles/GetChannelProfilesResponse";
import { useEmployeeHttpClient } from "../../context/employee/EmployeeContextProvider";

export const useChannelProfilesApi = () => {
    const httpClient = useEmployeeHttpClient();

    const get = async (request: GetChannelProfilesRequest) => {
        const queryParams = new URLSearchParams();
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        queryParams.set("page", request.page.toString());
        request.ids?.filter(id => !!id).forEach((id, i) => queryParams.set(`ids[${i}]`, id));

        const url = new URL(`api/channelprofiles?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetChannelProfilesResponse>(url, {});
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient]);

    return state;
}