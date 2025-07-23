import { useMemo } from "react";
import { GetCustomChargeMethodsRequest } from "./Dtos/customchargemethods/GetCustomChargeMethodsRequest";
import { GetCustomChargeMethodsResponse } from "./Dtos/customchargemethods/GetCustomChargeMethodsResponse";
import { CreateCustomChargeMethodRequest } from "./Dtos/customchargemethods/CreateCustomChargeMethodRequest";
import { CreateCustomChargeMethodResponse } from "./Dtos/customchargemethods/CreateCustomChargeMethodResponse";
import { PatchCustomChargeMethodRequest } from "./Dtos/customchargemethods/PatchCustomChargeMethodRequest";
import { PatchCustomChargeMethodResponse } from "./Dtos/customchargemethods/PatchCustomChargeMethodResponse";
import { DeleteCustomChargeMethodRequest } from "./Dtos/customchargemethods/DeleteCustomChargeMethodRequest";
import { DeleteCustomChargeMethodResponse } from "./Dtos/customchargemethods/DeleteCustomChargeMethodResponse";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";

export const useCustomChargeMethodsApi = () => {
    const client = useAuthenticatedHttpClient();

    const get = (request: GetCustomChargeMethodsRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        
        request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));

        const url = new URL(`api/customchargemethods?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetCustomChargeMethodsResponse>(url, {});
    }

    const create = (request: CreateCustomChargeMethodRequest) => {
        const url = new URL(`api/customchargemethods`, import.meta.env.VITE_API_URL).toString();
        return client.post<CreateCustomChargeMethodResponse>(url, request, {});
    }

    const patch = (request: PatchCustomChargeMethodRequest) => {
        const url = new URL(`api/customchargemethods/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return client.patch<PatchCustomChargeMethodResponse>(url, request, {});
    }

    const deleteCustomChargeMethod = (request: DeleteCustomChargeMethodRequest) => {
        const url = new URL(`api/customchargemethods/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return client.delete<DeleteCustomChargeMethodResponse>(url, request, {});
    }

    const state = useMemo(() => ({
        get,
        create,
        patch,
        delete: deleteCustomChargeMethod,
    }), [client]);

    return state;
}