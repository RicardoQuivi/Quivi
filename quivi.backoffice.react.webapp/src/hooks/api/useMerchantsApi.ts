import { useMemo } from "react";
import { GetMerchantsRequest } from "./Dtos/merchants/GetMerchantsRequest";
import { useHttpClient } from "./useHttpClient";
import { GetMerchantsResponse } from "./Dtos/merchants/GetMerchantsResponse";
import { CreateMerchantRequest } from "./Dtos/merchants/CreateMerchantRequest";
import { CreateMerchantResponse } from "./Dtos/merchants/CreateMerchantResponse";
import { PatchMerchantResponse } from "./Dtos/merchants/PatchMerchantResponse";
import { PatchMerchantRequest } from "./Dtos/merchants/PatchMerchantRequest";

export const useMerchantsApi = (token?: string) => {
    const httpClient = useHttpClient();
    
    const get = (request: GetMerchantsRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        if (!!request.search) {
            queryParams.set("search", request.search.toString());
        }

        if (!!request.parentId) {
            queryParams.set("parentid", request.parentId.toString());
        }

        let url = `${import.meta.env.VITE_API_URL}api/merchants?${queryParams}`;
        return httpClient.httpGet<GetMerchantsResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const create = (request: CreateMerchantRequest) => {
        return httpClient.httpPost<CreateMerchantResponse>(`${import.meta.env.VITE_API_URL}api/merchants`, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const patch = (request: PatchMerchantRequest) => {
        return httpClient.httpPatch<PatchMerchantResponse>(`${import.meta.env.VITE_API_URL}api/merchants/${request.id}`, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        get,
        create,
        patch,
    }), [httpClient, token]);

    return state;
}