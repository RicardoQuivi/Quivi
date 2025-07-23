import { useMemo } from "react";
import { GetPrinterWorkersRequest } from "./Dtos/printerWorkers/GetPrinterWorkersRequest";
import { GetPrinterWorkersResponse } from "./Dtos/printerWorkers/GetPrinterWorkersResponse";
import { CreatePrinterWorkerRequest } from "./Dtos/printerWorkers/CreatePrinterWorkerRequest";
import { CreatePrinterWorkerResponse } from "./Dtos/printerWorkers/CreatePrinterWorkerResponse";
import { PatchPrinterWorkerRequest } from "./Dtos/printerWorkers/PatchPrinterWorkerRequest";
import { PatchPrinterWorkerResponse } from "./Dtos/printerWorkers/PatchPrinterWorkerResponse";
import { DeletePrinterWorkerRequest } from "./Dtos/printerWorkers/DeletePrinterWorkerRequest";
import { DeletePrinterWorkerResponse } from "./Dtos/printerWorkers/DeletePrinterWorkerResponse";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";

export const usePrinterWorkersApi = () => {
    const client = useAuthenticatedHttpClient();

    const state = useMemo(() => ({
        get: (request: GetPrinterWorkersRequest) => {
            const queryParams = new URLSearchParams();
            queryParams.set("page", request.page.toString());
            if(request.pageSize != undefined) {
                queryParams.set("pageSize", request.pageSize.toString());
            }
            
            request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));

            const url = new URL(`api/printerworkers?${queryParams}`, import.meta.env.VITE_API_URL).toString();
            return client.get<GetPrinterWorkersResponse>(url, {});
        },
        create: (request: CreatePrinterWorkerRequest) => {
            const url = new URL(`api/printerworkers`, import.meta.env.VITE_API_URL).toString();
            return client.post<CreatePrinterWorkerResponse>(url, request, {});
        },
        patch: (request: PatchPrinterWorkerRequest) => {
            const url = new URL(`api/printerworkers/${request.id}`, import.meta.env.VITE_API_URL).toString();
            return client.patch<PatchPrinterWorkerResponse>(url, request, {});
        },
        delete: (request: DeletePrinterWorkerRequest) => {
            const url = new URL(`api/printerworkers/${request.id}`, import.meta.env.VITE_API_URL).toString();
            return client.delete<DeletePrinterWorkerResponse>(url, request, {});
        },
    }), [client]);

    return state;
}