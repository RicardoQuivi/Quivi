import { useMemo } from "react";
import { useEmployeeHttpClient } from "../../context/employee/EmployeeContextProvider";
import { GetTransactionItemsRequest } from "./Dtos/transactionItems/GetTransactionItemsRequest";
import { GetTransactionItemsResponse } from "./Dtos/transactionItems/GetTransactionItemsResponse";

export const useTransactionItemsApi = () => {
    const httpClient = useEmployeeHttpClient();

    const get = async (request: GetTransactionItemsRequest) => {
        const queryParams = new URLSearchParams();
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        queryParams.set("page", request.page.toString());

        const url = new URL(`api/transactions/${request.transactionId}/items?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetTransactionItemsResponse>(url, {});
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient]);

    return state;
}