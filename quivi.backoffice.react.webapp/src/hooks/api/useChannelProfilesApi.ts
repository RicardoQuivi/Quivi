import { useMemo } from "react";
import { GetChannelProfilesRequest } from "./Dtos/channelProfiles/GetChannelProfilesRequest";
import { GetChannelProfilesResponse } from "./Dtos/channelProfiles/GetChannelProfilesResponse";
import { CreateChannelProfileRequest } from "./Dtos/channelProfiles/CreateChannelProfileRequest";
import { CreateChannelProfileResponse } from "./Dtos/channelProfiles/CreateChannelProfileResponse";
import { PatchChannelProfileRequest } from "./Dtos/channelProfiles/PatchChannelProfileRequest";
import { PatchChannelProfileResponse } from "./Dtos/channelProfiles/PatchChannelProfileResponse";
import { DeleteChannelProfileRequest } from "./Dtos/channelProfiles/DeleteChannelProfileRequest";
import { DeleteChannelProfileResponse } from "./Dtos/channelProfiles/DeleteChannelProfileResponse";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";

export const useChannelProfilesApi = () => {   
    const client = useAuthenticatedHttpClient();

    const get = (request: GetChannelProfilesRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        
        request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));
        request.channelIds?.filter(id => !!id).forEach((id, index) => queryParams.set(`channelIds[${index}]`, id));

        const url = new URL(`api/channelprofiles?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetChannelProfilesResponse>(url, {});
    }

    const create = (request: CreateChannelProfileRequest) => {
        const url = new URL(`api/channelprofiles`, import.meta.env.VITE_API_URL).toString();
        return client.post<CreateChannelProfileResponse>(url, request, {});
    }

    const patch = (request: PatchChannelProfileRequest) => {
        const url = new URL(`api/channelprofiles/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return client.patch<PatchChannelProfileResponse>(url, request, {});
    }

    const deleteChannel = (request: DeleteChannelProfileRequest) => {
        const url = new URL(`api/channelprofiles/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return client.delete<DeleteChannelProfileResponse>(url, {}, {});
    }

    const state = useMemo(() => ({
        get,
        create,
        patch,
        delete: deleteChannel,
    }), [client]);

    return state;
}