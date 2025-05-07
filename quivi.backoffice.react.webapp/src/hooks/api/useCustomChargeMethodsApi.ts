import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetCustomChargeMethodsRequest } from "./Dtos/customchargemethods/GetCustomChargeMethodsRequest";
import { GetCustomChargeMethodsResponse } from "./Dtos/customchargemethods/GetCustomChargeMethodsResponse";
import { CreateCustomChargeMethodRequest } from "./Dtos/customchargemethods/CreateCustomChargeMethodRequest";
import { CreateCustomChargeMethodResponse } from "./Dtos/customchargemethods/CreateCustomChargeMethodResponse";
import { PatchCustomChargeMethodRequest } from "./Dtos/customchargemethods/PatchCustomChargeMethodRequest";
import { PatchCustomChargeMethodResponse } from "./Dtos/customchargemethods/PatchCustomChargeMethodResponse";
import { DeleteCustomChargeMethodRequest } from "./Dtos/customchargemethods/DeleteCustomChargeMethodRequest";
import { DeleteCustomChargeMethodResponse } from "./Dtos/customchargemethods/DeleteCustomChargeMethodResponse";

export const useCustomChargeMethodsApi = (token?: string) => {
    const httpClient = useHttpClient();
 
    const get = (request: GetCustomChargeMethodsRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        
        request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));
        let url = `${import.meta.env.VITE_API_URL}api/customchargemethods?${queryParams}`;
        return httpClient.httpGet<GetCustomChargeMethodsResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const create = (request: CreateCustomChargeMethodRequest) => {
        return httpClient.httpPost<CreateCustomChargeMethodResponse>(`${import.meta.env.VITE_API_URL}api/customchargemethods`, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const patch = (request: PatchCustomChargeMethodRequest) => {
        return httpClient.httpPatch<PatchCustomChargeMethodResponse>(`${import.meta.env.VITE_API_URL}api/customchargemethods/${request.id}`, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const deleteCustomChargeMethod = (request: DeleteCustomChargeMethodRequest) => {
        return httpClient.httpDelete<DeleteCustomChargeMethodResponse>(`${import.meta.env.VITE_API_URL}api/customchargemethods/${request.id}`, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        get,
        create,
        patch,
        delete: deleteCustomChargeMethod,
    }), [httpClient, token]);

    return state;
}