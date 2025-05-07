import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { CreateTransactionRequest } from "./Dtos/transactions/CreateTransactionRequest";
import { CreateTransactionResponse } from "./Dtos/transactions/CreateTransactionResponse";
import { GetTransactionsRequest } from "./Dtos/transactions/GetTransactionsRequest";
import { GetTransactionsResponse } from "./Dtos/transactions/GetTransactionsResponse";

export const useTransactionsApi = (token: string) => {
    const httpClient = useHttpClient();

    const get = async (request: GetTransactionsRequest) => {
        const queryParams = new URLSearchParams();
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        queryParams.set("page", request.page.toString());

        request.ids?.forEach((id, i) => queryParams.set(`ids[${i}]`, id));

        let url = `${import.meta.env.VITE_API_URL}api/transactions`;
        return httpClient.httpGet<GetTransactionsResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const create = async (request: CreateTransactionRequest) => {
        let url = `${import.meta.env.VITE_API_URL}api/transactions`;
        return httpClient.httpPost<CreateTransactionResponse>(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        get,
        create,
    }), [httpClient, token]);

    return state;
}