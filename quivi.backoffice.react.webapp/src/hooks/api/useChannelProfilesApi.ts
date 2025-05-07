import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetChannelProfilesRequest } from "./Dtos/channelProfiles/GetChannelProfilesRequest";
import { GetChannelProfilesResponse } from "./Dtos/channelProfiles/GetChannelProfilesResponse";
import { CreateChannelProfileRequest } from "./Dtos/channelProfiles/CreateChannelProfileRequest";
import { CreateChannelProfileResponse } from "./Dtos/channelProfiles/CreateChannelProfileResponse";
import { PatchChannelProfileRequest } from "./Dtos/channelProfiles/PatchChannelProfileRequest";
import { PatchChannelProfileResponse } from "./Dtos/channelProfiles/PatchChannelProfileResponse";
import { DeleteChannelProfileRequest } from "./Dtos/channelProfiles/DeleteChannelProfileRequest";
import { DeleteChannelProfileResponse } from "./Dtos/channelProfiles/DeleteChannelProfileResponse";

export const useChannelProfilesApi = (token?: string) => {
    const httpClient = useHttpClient();
    
    const get = (request: GetChannelProfilesRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        
        request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));
        request.channelIds?.filter(id => !!id).forEach((id, index) => queryParams.set(`channelIds[${index}]`, id));

        let url = `${import.meta.env.VITE_API_URL}api/channelprofiles?${queryParams}`;
        return httpClient.httpGet<GetChannelProfilesResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const create = (request: CreateChannelProfileRequest) => {
        let url = `${import.meta.env.VITE_API_URL}api/channelprofiles`;
        return httpClient.httpPost<CreateChannelProfileResponse>(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const patch = (request: PatchChannelProfileRequest) => {
        let url = `${import.meta.env.VITE_API_URL}api/channelprofiles/${request.id}`;
        return httpClient.httpPatch<PatchChannelProfileResponse>(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const deleteChannel = (request: DeleteChannelProfileRequest) => {
        let url = `${import.meta.env.VITE_API_URL}api/channelprofiles/${request.id}`;
        return httpClient.httpDelete<DeleteChannelProfileResponse>(url, {}, {
            "Authorization": `Bearer ${token}`,
        });
    }


    const state = useMemo(() => ({
        get,
        create,
        patch,
        delete: deleteChannel,
    }), [httpClient, token]);

    return state;
}