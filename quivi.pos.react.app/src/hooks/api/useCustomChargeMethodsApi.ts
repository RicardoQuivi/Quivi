import { useMemo } from "react";
import { GetCustomChargeMethodsResponse } from "./Dtos/customchargemethods/GetCustomChargeMethodsResponse";
import { GetCustomChargeMethodsRequest } from "./Dtos/customchargemethods/GetCustomChargeMethodsRequest";
import { useEmployeeHttpClient } from "../../context/employee/EmployeeContextProvider";

export const useCustomChargeMethodsApi = () => {
    const httpClient = useEmployeeHttpClient();

    const get = async (request: GetCustomChargeMethodsRequest) => {
        const queryParams = new URLSearchParams();
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        request.ids?.forEach((id, i) => queryParams.set(`ids[${i}]`, id));

        const url = new URL(`api/customchargemethods?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetCustomChargeMethodsResponse>(url, {});
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient]);

    return state;
}