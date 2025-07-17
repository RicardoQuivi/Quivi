import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { useTranslation } from "react-i18next";
import type { GetPaymentMethodsRequest } from "./Dtos/paymentmethods/GetPaymentMethodsRequest";
import type { GetPaymentMethodsResponse } from "./Dtos/paymentmethods/GetPaymentMethodsResponse";

export const usePaymentMethodsApi = () => {
    const httpClient = useHttpClient();
    const { i18n } = useTranslation();

    const get = (request: GetPaymentMethodsRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        const url = new URL(`api/paymentmethods/${request.channelId}?${queryParams.toString()}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpGet<GetPaymentMethodsResponse>(url, {
            'Accept-Language': i18n.language,
        });
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient, i18n, i18n.language]);

    return state;
}