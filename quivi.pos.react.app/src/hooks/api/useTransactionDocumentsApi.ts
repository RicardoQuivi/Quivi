import { useMemo } from "react";
import { useEmployeeHttpClient } from "../../context/employee/EmployeeContextProvider";
import { GetTransactionDocumentsRequest } from "./Dtos/transactionDocuments/GetTransactionDocumentsRequest";
import { GetTransactionDocumentsResponse } from "./Dtos/transactionDocuments/GetTransactionDocumentsResponse";

export const useTransactionDocumentsApi = () => {
    const httpClient = useEmployeeHttpClient();

    const get = async (request: GetTransactionDocumentsRequest) => {
        const queryParams = new URLSearchParams();
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        queryParams.set("page", request.page.toString());

        const url = new URL(`api/transactions/${request.transactionId}/documents?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetTransactionDocumentsResponse>(url, {});
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient]);

    return state;
}