import { useMemo } from "react";
import { GetPrintersRequest } from "./Dtos/printers/GetPrintersRequest";
import { GetPrintersResponse } from "./Dtos/printers/GetPrintersResponse";
import { useEmployeeHttpClient } from "../../context/employee/EmployeeContextProvider";

export const usePrinterApi = () => {
    const httpClient = useEmployeeHttpClient();

    const get = async (request: GetPrintersRequest) => {
        const queryParams = new URLSearchParams();
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        queryParams.set("page", request.page.toString());

        const url = new URL(`api/printers?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetPrintersResponse>(url, {});
    }
    const state = useMemo(() => ({
        get,
    }), [httpClient]);

    return state;
}