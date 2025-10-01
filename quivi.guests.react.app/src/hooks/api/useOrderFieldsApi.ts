import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { useTranslation } from "react-i18next";
import type { GetOrderFieldsResponse } from "./Dtos/orderFields/GetOrderFieldsResponse";
import type { GetOrderFieldsRequest } from "./Dtos/orderFields/GetOrderFieldsRequest";

export const useOrderFieldsApi = () => {
    const httpClient = useHttpClient();
    const { i18n } = useTranslation();

    const get = (request: GetOrderFieldsRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set(`channelId`, request.channelId);
        const url = new URL(`api/OrderFields?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetOrderFieldsResponse>(url, {
            'Accept-Language': i18n.language,
        });
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient, i18n, i18n.language]);

    return state;
}