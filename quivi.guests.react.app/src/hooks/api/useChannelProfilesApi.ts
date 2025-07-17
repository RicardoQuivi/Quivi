import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import type { GetChannelProfileResponse } from "./Dtos/channelProfiles/GetChannelProfileResponse";

export const useChannelProfilesApi = () => {
    const httpClient = useHttpClient();
    
    const get = (channelProfileId: string) => {
        const url = new URL(`api/channelprofiles/${channelProfileId}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpGet<GetChannelProfileResponse>(url);
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient]);

    return state;
}