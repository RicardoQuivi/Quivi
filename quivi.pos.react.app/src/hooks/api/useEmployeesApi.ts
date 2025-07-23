import { useMemo } from "react";
import { GetEmployeesRequest } from "./Dtos/employees/GetEmployeesRequest";
import { UpdateEmployeePinCodeRequest } from "./Dtos/employees/UpdateEmployeePinCodeRequest";
import { GetEmployeesResponse } from "./Dtos/employees/GetEmployeesResponse";
import { UpdateEmployeePinCodeResponse } from "./Dtos/employees/UpdateEmployeePinCodeResponse";
import { useAuthenticatedHttpClient } from "../../context/AuthContextProvider";

export const useEmployeesApi = () => {
    const httpClient = useAuthenticatedHttpClient();

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

        const url = new URL(`api/employees?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetEmployeesResponse>(url, {});
    }

    const updatePinCode = async (request: UpdateEmployeePinCodeRequest) => {
        const url = new URL(`api/employees/${request.id}/pincode`, import.meta.env.VITE_API_URL).toString();
        return await httpClient.put<UpdateEmployeePinCodeResponse>(url, request, {});
    }

    const state = useMemo(() => ({
        get,
        updatePinCode,
    }), [httpClient]);

    return state;
}