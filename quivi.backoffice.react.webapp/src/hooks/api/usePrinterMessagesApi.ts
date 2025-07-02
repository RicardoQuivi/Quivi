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
            
            let url = `${import.meta.env.VITE_API_URL}api/printermessages?${queryParams}`;
            return httpClient.httpGet<GetPrinterMessagesResponse>(url, {
                "Authorization": `Bearer ${token}`,
            });
        },
        create: (request: CreatePrinterMessageRequest) => {
            return httpClient.httpPost<CreatePrinterMessageResponse>(`${import.meta.env.VITE_API_URL}api/printermessages`, request, {
                "Authorization": `Bearer ${token}`,
            });
        },
    }), [httpClient, token]);

    return state;
}