import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetEmployeesRequest } from "./Dtos/employees/GetEmployeesRequest";
import { UpdateEmployeePinCodeRequest } from "./Dtos/employees/UpdateEmployeePinCodeRequest";
import { GetEmployeesResponse } from "./Dtos/employees/GetEmployeesResponse";
import { UpdateEmployeePinCodeResponse } from "./Dtos/employees/UpdateEmployeePinCodeResponse";

export const useEmployeesApi = (token: string | undefined) => {
    const httpClient = useHttpClient();

    const get = async (request: GetEmployeesRequest) => {
        const queryParams = new URLSearchParams();
        
        if(request.ids != undefined) {
            for(let i = 0; i < request.ids.length; ++i) {
                queryParams.set(`ids[${i}]`, request.ids[i].toString());
            }
        }

        if(request.page != undefined) {
            queryParams.set(`page`, request.page.toString());
        }

        if(request.pageSize != undefined) {
            queryParams.set(`pageSize`, request.pageSize.toString());
        }

        if(request.includeDeleted == true) {
            queryParams.set("includeDeleted", true.toString());
        }

        let url = `${import.meta.env.VITE_API_URL}api/employees?${queryParams}`;
        return httpClient.httpGet<GetEmployeesResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const updatePinCode = async (request: UpdateEmployeePinCodeRequest) => {
        const url = `${import.meta.env.VITE_API_URL}api/employees/${request.id}/pincode`;
        return await httpClient.httpPut<UpdateEmployeePinCodeResponse>(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        get,
        updatePinCode,
    }), [httpClient, token]);

    return state;
}