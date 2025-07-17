import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetPrinterWorkersRequest } from "./Dtos/printerWorkers/GetPrinterWorkersRequest";
import { GetPrinterWorkersResponse } from "./Dtos/printerWorkers/GetPrinterWorkersResponse";
import { CreatePrinterWorkerRequest } from "./Dtos/printerWorkers/CreatePrinterWorkerRequest";
import { CreatePrinterWorkerResponse } from "./Dtos/printerWorkers/CreatePrinterWorkerResponse";
import { PatchPrinterWorkerRequest } from "./Dtos/printerWorkers/PatchPrinterWorkerRequest";
import { PatchPrinterWorkerResponse } from "./Dtos/printerWorkers/PatchPrinterWorkerResponse";
import { DeletePrinterWorkerRequest } from "./Dtos/printerWorkers/DeletePrinterWorkerRequest";
import { DeletePrinterWorkerResponse } from "./Dtos/printerWorkers/DeletePrinterWorkerResponse";

export const usePrinterWorkersApi = (token?: string) => {
    const httpClient = useHttpClient();
 
    const state = useMemo(() => ({
        get: (request: GetPrinterWorkersRequest) => {
            const queryParams = new URLSearchParams();
            queryParams.set("page", request.page.toString());
            if(request.pageSize != undefined) {
                queryParams.set("pageSize", request.pageSize.toString());
            }
            
            request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));

            const url = new URL(`api/printerworkers?${queryParams}`, import.meta.env.VITE_API_URL).toString();
            return httpClient.httpGet<GetPrinterWorkersResponse>(url, {
                "Authorization": `Bearer ${token}`,
            });
        },
        create: (request: CreatePrinterWorkerRequest) => {
            const url = new URL(`api/printerworkers`, import.meta.env.VITE_API_URL).toString();
            return httpClient.httpPost<CreatePrinterWorkerResponse>(url, request, {
                "Authorization": `Bearer ${token}`,
            });
        },
        patch: (request: PatchPrinterWorkerRequest) => {
            const url = new URL(`api/printerworkers/${request.id}`, import.meta.env.VITE_API_URL).toString();
            return httpClient.httpPatch<PatchPrinterWorkerResponse>(url, request, {
                "Authorization": `Bearer ${token}`,
            });
        },
        delete: (request: DeletePrinterWorkerRequest) => {
            const url = new URL(`api/printerworkers/${request.id}`, import.meta.env.VITE_API_URL).toString();
            return httpClient.httpDelete<DeletePrinterWorkerResponse>(url, request, {
                "Authorization": `Bearer ${token}`,
            });
        },
    }), [httpClient, token]);

    return state;
}