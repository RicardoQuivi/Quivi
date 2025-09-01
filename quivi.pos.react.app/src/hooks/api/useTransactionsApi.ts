import { useMemo } from "react";
import { CreateTransactionRequest } from "./Dtos/transactions/CreateTransactionRequest";
import { CreateTransactionResponse } from "./Dtos/transactions/CreateTransactionResponse";
import { GetTransactionsRequest } from "./Dtos/transactions/GetTransactionsRequest";
import { GetTransactionsResponse } from "./Dtos/transactions/GetTransactionsResponse";
import { useEmployeeHttpClient } from "../../context/employee/EmployeeContextProvider";
import { GetTransactionsResumeRequest } from "./Dtos/transactions/GetTransactionsResumeRequest";
import { GetTransactionsResumeResponse } from "./Dtos/transactions/GetTransactionsResumeResponse";

export const useTransactionsApi = () => {
    const httpClient = useEmployeeHttpClient();

    const get = async (request: GetTransactionsRequest) => {
        const queryParams = new URLSearchParams();
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        queryParams.set("page", request.page.toString());

        request.ids?.forEach((id, i) => queryParams.set(`ids[${i}]`, id));

        const url = new URL(`api/transactions?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetTransactionsResponse>(url, {});
    }

    const getResume = async (request: GetTransactionsResumeRequest) => {
        const queryParams = new URLSearchParams();

        request.ids?.forEach((id, i) => queryParams.set(`ids[${i}]`, id));

        const url = new URL(`api/transactions/resume?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetTransactionsResumeResponse>(url, {});
    }

    const create = async (request: CreateTransactionRequest) => {
        const url = new URL(`api/transactions`, import.meta.env.VITE_API_URL).toString();
        return httpClient.post<CreateTransactionResponse>(url, request, {});
    }

    const state = useMemo(() => ({
        get,
        getResume,
        create,
    }), [httpClient]);

    return state;
}