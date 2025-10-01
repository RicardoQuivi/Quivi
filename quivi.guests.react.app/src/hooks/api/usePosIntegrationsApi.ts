import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import type { GetPosIntegrationResponse } from "./Dtos/posIntegrations/GetPosIntegrationResponse";

export const usePosIntegrationsApi = () => {
    const httpClient = useHttpClient();
    
    const get = (posIntegrationId: string) => {
        const url = new URL(`api/posintegrations/${posIntegrationId}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetPosIntegrationResponse>(url);
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient]);

    return state;
}