import { useMemo } from "react";
import { GetMerchantsRequest } from "./Dtos/merchants/GetMerchantsRequest";
import { GetMerchantsResponse } from "./Dtos/merchants/GetMerchantsResponse";
import { CreateMerchantRequest } from "./Dtos/merchants/CreateMerchantRequest";
import { CreateMerchantResponse } from "./Dtos/merchants/CreateMerchantResponse";
import { PatchMerchantResponse } from "./Dtos/merchants/PatchMerchantResponse";
import { PatchMerchantRequest } from "./Dtos/merchants/PatchMerchantRequest";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";

export const useMerchantsApi = () => {
    const client = useAuthenticatedHttpClient();

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
            queryParams.set("parentId", request.parentId.toString());
        }

        request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));

        const url = new URL(`api/merchants?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetMerchantsResponse>(url, {});
    }

    const create = (request: CreateMerchantRequest) => {
        const url = new URL(`api/merchants`, import.meta.env.VITE_API_URL).toString();
        return client.post<CreateMerchantResponse>(url, request, {});
    }

    const patch = (request: PatchMerchantRequest) => {
        const url = new URL(`api/merchants/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return client.patch<PatchMerchantResponse>(url, request, {});
    }

    const state = useMemo(() => ({
        get,
        create,
        patch,
    }), [client]);

    return state;
}