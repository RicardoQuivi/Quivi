import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { useTranslation } from "react-i18next";
import type { GetTransactionsRequest } from "./Dtos/transactions/GetTransactionsRequest";
import type { GetTransactionsResponse } from "./Dtos/transactions/GetTransactionsResponse";
import type { CreateTransactionRequest } from "./Dtos/transactions/CreateTransactionRequest";
import type { CreateTransactionResponse } from "./Dtos/transactions/CreateTransactionResponse";
import type { ProcessTransactionResponse } from "./Dtos/transactions/ProcessTransactionResponse";

export const useTransactionsApi = () => {
    const httpClient = useHttpClient();
    const { i18n } = useTranslation();

    const get = (request: GetTransactionsRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        if(request.sessionId != undefined) {
            queryParams.set("sessionId", request.sessionId);
        }

        if(request.id != undefined) {
            queryParams.set("id", request.id);
        }

        if(request.orderId != undefined) {
            queryParams.set("orderId", request.orderId);
        }

        const url = new URL(`api/transactions?${queryParams.toString()}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpGet<GetTransactionsResponse>(url, {
            'Accept-Language': i18n.language,
        });
    }

    const create = (request: CreateTransactionRequest) => {
        const url = new URL(`api/transactions`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpPost<CreateTransactionResponse>(url, request, {
            'Accept-Language': i18n.language,
        });
    }

    const processCash = (id: string) => {
        const url = new URL(`api/transactions/${id}/Cash`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpPut<ProcessTransactionResponse>(url, {}, {
            'Accept-Language': i18n.language,
        });
    }

    const state = useMemo(() => ({
        get,
        create,
        processCash,
    }), [httpClient, i18n, i18n.language]);

    return state;
}