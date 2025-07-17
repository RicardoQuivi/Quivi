import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetPrintersRequest } from "./Dtos/printers/GetPrintersRequest";
import { GetPrintersResponse } from "./Dtos/printers/GetPrintersResponse";
import { CreatePrinterRequest } from "./Dtos/printers/CreatePrinterRequest";
import { CreatePrinterResponse } from "./Dtos/printers/CreatePrinterResponse";
import { PatchPrinterRequest } from "./Dtos/printers/PatchPrinterRequest";
import { PatchPrinterResponse } from "./Dtos/printers/PatchPrinterResponse";
import { DeletePrinterRequest } from "./Dtos/printers/DeletePrinterRequest";
import { DeletePrinterResponse } from "./Dtos/printers/DeletePrinterResponse";

export const usePrintersApi = (token?: string) => {
    const httpClient = useHttpClient();
 
    const state = useMemo(() => ({
        get: (request: GetPrintersRequest) => {
            const queryParams = new URLSearchParams();
            queryParams.set("page", request.page.toString());
            if(request.pageSize != undefined) {
                queryParams.set("pageSize", request.pageSize.toString());
            }

            request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));

            if(request.printerWorkerId != undefined) {
                queryParams.set("printerWorkerId", request.printerWorkerId);
            }
            
            const url = new URL(`api/printers?${queryParams}`, import.meta.env.VITE_API_URL).toString();
            return httpClient.httpGet<GetPrintersResponse>(url, {
                "Authorization": `Bearer ${token}`,
            });
        },
        create: (request: CreatePrinterRequest) => {
            const url = new URL(`api/printers`, import.meta.env.VITE_API_URL).toString();
            return httpClient.httpPost<CreatePrinterResponse>(url, request, {
                "Authorization": `Bearer ${token}`,
            });
        },
        patch: (request: PatchPrinterRequest) => {
            const url = new URL(`api/printers/${request.id}`, import.meta.env.VITE_API_URL).toString();
            return httpClient.httpPatch<PatchPrinterResponse>(url, request, {
                "Authorization": `Bearer ${token}`,
            });
        },
        delete: (request: DeletePrinterRequest) => {
            const url = new URL(`api/printers/${request.id}`, import.meta.env.VITE_API_URL).toString();
            return httpClient.httpDelete<DeletePrinterResponse>(url, request, {
                "Authorization": `Bearer ${token}`,
            });
        },
    }), [httpClient, token]);

    return state;
}