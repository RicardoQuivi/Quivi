import { useMemo } from "react";
import { GetPrintersRequest } from "./Dtos/printers/GetPrintersRequest";
import { GetPrintersResponse } from "./Dtos/printers/GetPrintersResponse";
import { CreatePrinterRequest } from "./Dtos/printers/CreatePrinterRequest";
import { CreatePrinterResponse } from "./Dtos/printers/CreatePrinterResponse";
import { PatchPrinterRequest } from "./Dtos/printers/PatchPrinterRequest";
import { PatchPrinterResponse } from "./Dtos/printers/PatchPrinterResponse";
import { DeletePrinterRequest } from "./Dtos/printers/DeletePrinterRequest";
import { DeletePrinterResponse } from "./Dtos/printers/DeletePrinterResponse";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";

export const usePrintersApi = () => {
    const client = useAuthenticatedHttpClient();

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
            return client.get<GetPrintersResponse>(url, {});
        },
        create: (request: CreatePrinterRequest) => {
            const url = new URL(`api/printers`, import.meta.env.VITE_API_URL).toString();
            return client.post<CreatePrinterResponse>(url, request, {});
        },
        patch: (request: PatchPrinterRequest) => {
            const url = new URL(`api/printers/${request.id}`, import.meta.env.VITE_API_URL).toString();
            return client.patch<PatchPrinterResponse>(url, request, {});
        },
        delete: (request: DeletePrinterRequest) => {
            const url = new URL(`api/printers/${request.id}`, import.meta.env.VITE_API_URL).toString();
            return client.delete<DeletePrinterResponse>(url, request, {});
        },
    }), [client]);

    return state;
}