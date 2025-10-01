import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import type { GetChannelResponse } from "./Dtos/channels/GetChannelResponse";

export const useChannelsApi = () => {
    const httpClient = useHttpClient();
    
    const get = (channelId: string) => {
        const url = new URL(`api/channels/${channelId}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetChannelResponse>(url);
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient]);

    return state;
}