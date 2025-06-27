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
            
            let url = `${import.meta.env.VITE_API_URL}api/printers?${queryParams}`;
            return httpClient.httpGet<GetPrintersResponse>(url, {
                "Authorization": `Bearer ${token}`,
            });
        },
        create: (request: CreatePrinterRequest) => {
            return httpClient.httpPost<CreatePrinterResponse>(`${import.meta.env.VITE_API_URL}api/printers`, request, {
                "Authorization": `Bearer ${token}`,
            });
        },
        patch: (request: PatchPrinterRequest) => {
            return httpClient.httpPatch<PatchPrinterResponse>(`${import.meta.env.VITE_API_URL}api/printers/${request.id}`, request, {
                "Authorization": `Bearer ${token}`,
            });
        },
        delete: (request: DeletePrinterRequest) => {
            return httpClient.httpDelete<DeletePrinterResponse>(`${import.meta.env.VITE_API_URL}api/printers/${request.id}`, request, {
                "Authorization": `Bearer ${token}`,
            });
        },
    }), [httpClient, token]);

    return state;
}