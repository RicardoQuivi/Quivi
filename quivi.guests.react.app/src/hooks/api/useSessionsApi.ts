import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { useTranslation } from "react-i18next";
import type { GetSessionRequest } from "./Dtos/sessions/GetSessionRequest";
import type { GetSessionResponse } from "./Dtos/sessions/GetSessionResponse";

export const useSessionsApi = () => {
    const httpClient = useHttpClient();
    const { i18n } = useTranslation();

    const get = (request: GetSessionRequest) => {
        const url = new URL(`api/sessions/${request.channelId}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpGet<GetSessionResponse>(url, {
            'Accept-Language': i18n.language,
        });
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient, i18n, i18n.language]);

    return state;
}