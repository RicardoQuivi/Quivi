import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetOrdersRequest } from "./Dtos/orders/GetOrdersRequest";
import { GetOrdersResponse } from "./Dtos/orders/GetOrdersResponse";
import { CreateOrdersRequest } from "./Dtos/orders/CreateOrdersRequest";
import { CreateOrdersResponse } from "./Dtos/orders/CreateOrderResponse";

export const useOrdersApi = (token: string) => {
    const httpClient = useHttpClient();

    const get = async (request: GetOrdersRequest) => {
        const queryParams = new URLSearchParams();
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        queryParams.set("page", request.page.toString());
        request.ids?.forEach((id, i) => queryParams.set(`ids[${i}]`, id));
        request.channelIds?.forEach((id, i) => queryParams.set(`channelIds[${i}]`, id));
        request.states?.forEach((s, i) => queryParams.set(`states[${i}]`, s.toString()));

        let url = `${import.meta.env.VITE_API_URL}api/orders?${queryParams}`;
        return httpClient.httpGet<GetOrdersResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const post = async (request: CreateOrdersRequest) => {
        let url = `${import.meta.env.VITE_API_URL}api/orders`;
        return httpClient.httpPost<CreateOrdersResponse>(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        get,
        post,
    }), [httpClient, token]);

    return state;
}