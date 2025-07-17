import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { CreatePrinterMessageRequest } from "./Dtos/printerMessages/CreatePrinterMessageRequest";
import { CreatePrinterMessageResponse } from "./Dtos/printerMessages/CreatePrinterMessageResponse";
import { GetPrinterMessagesResponse } from "./Dtos/printerMessages/GetPrinterMessagesResponse";
import { GetPrinterMessagesRequest } from "./Dtos/printerMessages/GetPrinterMessagesRequest";

export const usePrinterMessagesApi = (token?: string) => {
    const httpClient = useHttpClient();
 
    const state = useMemo(() => ({
        get: (request: GetPrinterMessagesRequest) => {
            const queryParams = new URLSearchParams();
            queryParams.set("page", request.page.toString());
            if(request.pageSize != undefined) {
                queryParams.set("pageSize", request.pageSize.toString());
            }

            if(request.printerId != undefined) {
                queryParams.set("printerId", request.printerId);
            }
            
            const url = new URL(`api/printermessages?${queryParams}`, import.meta.env.VITE_API_URL).toString();
            return httpClient.httpGet<GetPrinterMessagesResponse>(url, {
                "Authorization": `Bearer ${token}`,
            });
        },
        create: (request: CreatePrinterMessageRequest) => {
            const url = new URL(`api/printermessages`, import.meta.env.VITE_API_URL).toString();
            return httpClient.httpPost<CreatePrinterMessageResponse>(url, request, {
                "Authorization": `Bearer ${token}`,
            });
        },
    }), [httpClient, token]);

    return state;
}