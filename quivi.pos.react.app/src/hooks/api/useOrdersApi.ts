import { useMemo } from "react";
import { GetOrdersRequest } from "./Dtos/orders/GetOrdersRequest";
import { GetOrdersResponse } from "./Dtos/orders/GetOrdersResponse";
import { CreateOrdersRequest } from "./Dtos/orders/CreateOrdersRequest";
import { CreateOrdersResponse } from "./Dtos/orders/CreateOrderResponse";
import { UpdateOrderToNextStateRequest } from "./Dtos/orders/UpdateOrderToNextStateRequest";
import { UpdateOrderToNextStateResponse } from "./Dtos/orders/UpdateOrderToNextStateResponse";
import { DeclineOrderRequest } from "./Dtos/orders/DeclineOrderRequest";
import { DeclineOrderResponse } from "./Dtos/orders/DeclineOrderResponse";
import { useEmployeeHttpClient } from "../../context/employee/EmployeeContextProvider";

export const useOrdersApi = () => {
    const httpClient = useEmployeeHttpClient();

    const get = async (request: GetOrdersRequest) => {
        const queryParams = new URLSearchParams();
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        queryParams.set("page", request.page.toString());
        queryParams.set("sortDirection", request.sortDirection.toString())
        request.ids?.forEach((id, i) => queryParams.set(`ids[${i}]`, id));
        request.channelIds?.forEach((id, i) => queryParams.set(`channelIds[${i}]`, id));
        request.states?.forEach((s, i) => queryParams.set(`states[${i}]`, s.toString()));
        request.sessionIds?.forEach((s, i) => queryParams.set(`sessionIds[${i}]`, s.toString()));

        const url = new URL(`api/orders?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetOrdersResponse>(url, {});
    }

    const post = async (request: CreateOrdersRequest) => {
        const url = new URL(`api/orders`, import.meta.env.VITE_API_URL).toString();
        return httpClient.post<CreateOrdersResponse>(url, request, {});
    }

    const processTo = async (request: UpdateOrderToNextStateRequest): Promise<UpdateOrderToNextStateResponse> => {
        const queryParams = new URLSearchParams();
        if(request.completeOrder != undefined) {
            queryParams.set("complete", request.completeOrder ? "true" : "false");
        }
        const url = new URL(`api/orders/${request.id}/next?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return await httpClient.post<UpdateOrderToNextStateResponse>(url, request, {});
    }

    const decline = async (request: DeclineOrderRequest): Promise<DeclineOrderResponse> => {
        const url = new URL(`api/orders/${request.id}/decline`, import.meta.env.VITE_API_URL).toString();
        return await httpClient.post<DeclineOrderResponse>(url, request, {});
    }

    const state = useMemo(() => ({
        get,
        post,
        processTo,
        decline,
    }), [httpClient]);

    return state;
}