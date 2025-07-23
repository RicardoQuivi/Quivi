import { useMemo } from "react";
import { GetChannelsRequest } from "./Dtos/channels/GetChannelsRequest";
import { GetChannelsResponse } from "./Dtos/channels/GetChannelsResponse";
import { CreateChannelsRequest } from "./Dtos/channels/CreateChannelRequest";
import { CreateChannelsResponse } from "./Dtos/channels/CreateChannelsResponse";
import { DeleteChannelsRequest } from "./Dtos/channels/DeleteChannelsRequest";
import { DeleteChannelsResponse } from "./Dtos/channels/DeleteChannelsResponse";
import { PatchChannelsRequest } from "./Dtos/channels/PatchChannelsRequest";
import { PatchChannelsResponse } from "./Dtos/channels/PatchChannelsResponse";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";

export const useChannelsApi = () => {
    const client = useAuthenticatedHttpClient();

    const get = (request: GetChannelsRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        const url = new URL(`api/channels?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetChannelsResponse>(url, {});
    }

    const create = (request: CreateChannelsRequest) => {
        const url = new URL(`api/channels`, import.meta.env.VITE_API_URL).toString();
        return client.post<CreateChannelsResponse>(url, request, {});
    }

    const patch = (request: PatchChannelsRequest) => {
        const url = new URL(`api/channels`, import.meta.env.VITE_API_URL).toString();
        return client.patch<PatchChannelsResponse>(url, request, {});
    }

    const deleteChannel = (request: DeleteChannelsRequest) => {
        const url = new URL(`api/channels`, import.meta.env.VITE_API_URL).toString();
        return client.delete<DeleteChannelsResponse>(url, request, {});
    }


    const state = useMemo(() => ({
        get,
        create,
        patch,
        delete: deleteChannel,
    }), [client]);

    return state;
}