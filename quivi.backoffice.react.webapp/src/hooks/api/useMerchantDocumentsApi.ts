import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetMerchantDocumentsRequest } from "./Dtos/merchantdocuments/GetMerchantDocumentsRequest";
import { GetMerchantDocumentsResponse } from "./Dtos/merchantdocuments/GetMerchantDocumentsResponse";

export const useMerchantDocumentsApi = (token?: string) => {
    const httpClient = useHttpClient();
 
    const get = (request: GetMerchantDocumentsRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        request.transactionIds?.forEach((id, index) => queryParams.set(`transactionIds[${index}]`, id));
        if(request.monthlyInvoiceOnly === true) {
            queryParams.set("monthlyInvoiceOnly", "true");
        }

        const url = new URL(`api/merchantDocuments?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpGet<GetMerchantDocumentsResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient, token]);

    return state;
}