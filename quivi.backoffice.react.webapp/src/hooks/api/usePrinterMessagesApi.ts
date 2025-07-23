import { useMemo } from "react";
import { CreatePrinterMessageRequest } from "./Dtos/printerMessages/CreatePrinterMessageRequest";
import { CreatePrinterMessageResponse } from "./Dtos/printerMessages/CreatePrinterMessageResponse";
import { GetPrinterMessagesResponse } from "./Dtos/printerMessages/GetPrinterMessagesResponse";
import { GetPrinterMessagesRequest } from "./Dtos/printerMessages/GetPrinterMessagesRequest";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";

export const usePrinterMessagesApi = () => {
    const client = useAuthenticatedHttpClient();

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
            return client.get<GetPrinterMessagesResponse>(url, {});
        },
        create: (request: CreatePrinterMessageRequest) => {
            const url = new URL(`api/printermessages`, import.meta.env.VITE_API_URL).toString();
            return client.post<CreatePrinterMessageResponse>(url, request, {});
        },
    }), [client]);

    return state;
}