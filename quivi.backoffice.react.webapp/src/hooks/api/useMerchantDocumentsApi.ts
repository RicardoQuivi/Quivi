import { useMemo } from "react";
import { GetMerchantDocumentsRequest } from "./Dtos/merchantdocuments/GetMerchantDocumentsRequest";
import { GetMerchantDocumentsResponse } from "./Dtos/merchantdocuments/GetMerchantDocumentsResponse";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";

export const useMerchantDocumentsApi = () => {
    const client = useAuthenticatedHttpClient();

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
        return client.get<GetMerchantDocumentsResponse>(url, {});
    }

    const state = useMemo(() => ({
        get,
    }), [client]);

    return state;
}