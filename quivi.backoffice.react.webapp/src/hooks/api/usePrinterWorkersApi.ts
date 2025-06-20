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
            request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));

            let url = `${import.meta.env.VITE_API_URL}api/printerworkers?${queryParams}`;
            return httpClient.httpGet<GetPrinterWorkersResponse>(url, {
                "Authorization": `Bearer ${token}`,
            });
        },
        create: (request: CreatePrinterWorkerRequest) => {
            return httpClient.httpPost<CreatePrinterWorkerResponse>(`${import.meta.env.VITE_API_URL}api/printerworkers`, request, {
                "Authorization": `Bearer ${token}`,
            });
        },
        patch: (request: PatchPrinterWorkerRequest) => {
            return httpClient.httpPatch<PatchPrinterWorkerResponse>(`${import.meta.env.VITE_API_URL}api/printerworkers/${request.id}`, request, {
                "Authorization": `Bearer ${token}`,
            });
        },
        delete: (request: DeletePrinterWorkerRequest) => {
            return httpClient.httpDelete<DeletePrinterWorkerResponse>(`${import.meta.env.VITE_API_URL}api/printerworkers/${request.id}`, request, {
                "Authorization": `Bearer ${token}`,
            });
        },
    }), [httpClient, token]);

    return state;
}