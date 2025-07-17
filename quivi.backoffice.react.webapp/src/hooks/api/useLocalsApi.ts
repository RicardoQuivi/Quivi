import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetLocalsRequest } from "./Dtos/locals/GetLocalsRequest";
import { GetLocalsResponse } from "./Dtos/locals/GetLocalsResponse";
import { CreateLocalRequest } from "./Dtos/locals/CreateLocalRequest";
import { CreateLocalResponse } from "./Dtos/locals/CreateLocalResponse";
import { PatchLocalRequest } from "./Dtos/locals/PatchLocalRequest";
import { PatchLocalResponse } from "./Dtos/locals/PatchLocalResponse";
import { DeleteLocalRequest } from "./Dtos/locals/DeleteLocalRequest";
import { DeleteLocalResponse } from "./Dtos/locals/DeleteLocalResponse";

export const useLocalsApi = (token?: string) => {
    const httpClient = useHttpClient();
 
    const get = (request: GetLocalsRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));

        const url = new URL(`api/locals?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpGet<GetLocalsResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const create = (request: CreateLocalRequest) => {
        const url = new URL(`api/locals`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpPost<CreateLocalResponse>(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const patch = (request: PatchLocalRequest) => {
        const url = new URL(`api/locals/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpPatch<PatchLocalResponse>(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const deleteLocal = (request: DeleteLocalRequest) => {
        const url = new URL(`api/locals/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpDelete<DeleteLocalResponse>(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        get,
        create,
        patch,
        delete: deleteLocal,
    }), [httpClient, token]);

    return state;
}