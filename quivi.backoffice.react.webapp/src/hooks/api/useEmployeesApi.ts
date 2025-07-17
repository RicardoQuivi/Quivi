import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetEmployeesRequest } from "./Dtos/employees/GetEmployeesRequest";
import { GetEmployeesResponse } from "./Dtos/employees/GetEmployeesResponse";
import { CreateEmployeeRequest } from "./Dtos/employees/CreateEmployeeRequest";
import { CreateEmployeeResponse } from "./Dtos/employees/CreateEmployeeResponse";
import { PatchEmployeeRequest } from "./Dtos/employees/PatchEmployeeRequest";
import { PatchEmployeeResponse } from "./Dtos/employees/PatchEmployeeResponse";
import { DeleteEmployeeRequest } from "./Dtos/employees/DeleteEmployeeRequest";
import { DeleteEmployeeResponse } from "./Dtos/employees/DeleteEmployeeResponse";
import { ResetPinCodeEmployeeResponse } from "./Dtos/employees/ResetPinCodeEmployeeResponse";
import { ResetPinCodeEmployeeRequest } from "./Dtos/employees/ResetPinCodeEmployeeRequest";

export const useEmployeesApi = (token?: string) => {
    const httpClient = useHttpClient();
 
    const get = (request: GetEmployeesRequest) => {
        const queryParams = new URLSearchParams();
        
        request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));

        const url = new URL(`api/employees?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpGet<GetEmployeesResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const create = (request: CreateEmployeeRequest) => {
        const url = new URL(`api/employees`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpPost<CreateEmployeeResponse>(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const patch = (request: PatchEmployeeRequest) => {
        const url = new URL(`api/employees/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpPatch<PatchEmployeeResponse>(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const deleteEmployee = (request: DeleteEmployeeRequest) => {
        const url = new URL(`api/employees/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpDelete<DeleteEmployeeResponse>(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const resetPinCode = (request: ResetPinCodeEmployeeRequest) => {
        const url = new URL(`api/employees/${request.id}/pincode`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpDelete<ResetPinCodeEmployeeResponse>(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        get,
        create,
        patch,
        delete: deleteEmployee,
        resetPinCode,
    }), [httpClient, token]);

    return state;
}