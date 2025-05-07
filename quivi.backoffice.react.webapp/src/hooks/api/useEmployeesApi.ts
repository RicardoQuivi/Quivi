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

        let url = `${import.meta.env.VITE_API_URL}api/employees?${queryParams}`;
        return httpClient.httpGet<GetEmployeesResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const create = (request: CreateEmployeeRequest) => {
        return httpClient.httpPost<CreateEmployeeResponse>(`${import.meta.env.VITE_API_URL}api/employees`, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const patch = (request: PatchEmployeeRequest) => {
        return httpClient.httpPatch<PatchEmployeeResponse>(`${import.meta.env.VITE_API_URL}api/employees/${request.id}`, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const deleteEmployee = (request: DeleteEmployeeRequest) => {
        return httpClient.httpDelete<DeleteEmployeeResponse>(`${import.meta.env.VITE_API_URL}api/employees/${request.id}`, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const resetPinCode = (request: ResetPinCodeEmployeeRequest) => {
        return httpClient.httpDelete<ResetPinCodeEmployeeResponse>(`${import.meta.env.VITE_API_URL}api/employees/${request.id}/pincode`, request, {
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