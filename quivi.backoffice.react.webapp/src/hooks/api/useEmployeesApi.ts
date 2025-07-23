import { useMemo } from "react";
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
import { useAuthenticatedHttpClient } from "../../context/AuthContext";

export const useEmployeesApi = () => {
    const client = useAuthenticatedHttpClient();

    const get = (request: GetEmployeesRequest) => {
        const queryParams = new URLSearchParams();
        
        request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));

        const url = new URL(`api/employees?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetEmployeesResponse>(url, {});
    }

    const create = (request: CreateEmployeeRequest) => {
        const url = new URL(`api/employees`, import.meta.env.VITE_API_URL).toString();
        return client.post<CreateEmployeeResponse>(url, request, {});
    }

    const patch = (request: PatchEmployeeRequest) => {
        const url = new URL(`api/employees/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return client.patch<PatchEmployeeResponse>(url, request, {});
    }

    const deleteEmployee = (request: DeleteEmployeeRequest) => {
        const url = new URL(`api/employees/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return client.delete<DeleteEmployeeResponse>(url, request, {});
    }

    const resetPinCode = (request: ResetPinCodeEmployeeRequest) => {
        const url = new URL(`api/employees/${request.id}/pincode`, import.meta.env.VITE_API_URL).toString();
        return client.delete<ResetPinCodeEmployeeResponse>(url, request, {});
    }

    const state = useMemo(() => ({
        get,
        create,
        patch,
        delete: deleteEmployee,
        resetPinCode,
    }), [client]);

    return state;
}