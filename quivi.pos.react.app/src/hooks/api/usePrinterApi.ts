import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetPrintersRequest } from "./Dtos/printers/GetPrintersRequest";
import { GetPrintersResponse } from "./Dtos/printers/GetPrintersResponse";

export const usePrinterApi = (token: string) => {
    const httpClient = useHttpClient();

    const get = async (request: GetPrintersRequest) => {
        const queryParams = new URLSearchParams();
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        queryParams.set("page", request.page.toString());

        const url = new URL(`api/printers?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpGet<GetPrintersResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }
    const state = useMemo(() => ({
        get,
    }), [httpClient, token]);

    return state;
}