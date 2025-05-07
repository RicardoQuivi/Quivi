import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetChannelsRequest } from "./Dtos/channels/GetChannelsRequest";
import { GetChannelsResponse } from "./Dtos/channels/GetChannelsResponse";
import { CreateChannelsRequest } from "./Dtos/channels/CreateChannelRequest";
import { CreateChannelsResponse } from "./Dtos/channels/CreateChannelsResponse";
import { DeleteChannelsRequest } from "./Dtos/channels/DeleteChannelsRequest";
import { DeleteChannelsResponse } from "./Dtos/channels/DeleteChannelsResponse";
import { PatchChannelsRequest } from "./Dtos/channels/PatchChannelsRequest";
import { PatchChannelsResponse } from "./Dtos/channels/PatchChannelsResponse";

export const useChannelsApi = (token?: string) => {
    const httpClient = useHttpClient();
    
    const get = (request: GetChannelsRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        let url = `${import.meta.env.VITE_API_URL}api/channels?${queryParams}`;
        return httpClient.httpGet<GetChannelsResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const create = (request: CreateChannelsRequest) => {
        let url = `${import.meta.env.VITE_API_URL}api/channels`;
        return httpClient.httpPost<CreateChannelsResponse>(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const patch = (request: PatchChannelsRequest) => {
        let url = `${import.meta.env.VITE_API_URL}api/channels`;
        return httpClient.httpPatch<PatchChannelsResponse>(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const deleteChannel = (request: DeleteChannelsRequest) => {
        let url = `${import.meta.env.VITE_API_URL}api/channels`;
        return httpClient.httpDelete<DeleteChannelsResponse>(url, request, {
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