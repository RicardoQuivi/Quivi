import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import type { GetOrdersRequest } from "./Dtos/orders/GetOrdersRequest";
import { useTranslation } from "react-i18next";
import type { GetOrdersResponse } from "./Dtos/orders/GetOrdersResponse";
import type { CreateOrderRequest } from "./Dtos/orders/CreateOrderRequest";
import type { CreateOrderResponse } from "./Dtos/orders/CreateOrderResponse";
import type { UpdateOrderRequest } from "./Dtos/orders/UpdateOrderRequest";
import type { UpdateOrderResponse } from "./Dtos/orders/UpdateOrderResponse";
import type { SubmitOrderRequest } from "./Dtos/orders/SubmitOrderRequest";
import type { SubmitOrderResponse } from "./Dtos/orders/SubmitOrderResponse";

export const useOrdersApi = () => {
    const httpClient = useHttpClient();
    const { i18n } = useTranslation();

    const get = (request: GetOrdersRequest) => {
         if(!request.chargeIds && !request.ids && !request.channelIds) {
            throw new Error("Invalid Arguments");
        }

        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        request.ids?.forEach((id, i) => queryParams.set(`ids[${i}]`, id));
        request.chargeIds?.forEach((c, i) => queryParams.set(`chargeIds[${i}]`, c));
        request.channelIds?.forEach((q, i) => queryParams.set(`channelIds[${i}]`, q));

        if(request.sessionId != undefined) {
            queryParams.set(`sessionId`, request.sessionId)
        }
        
        const url = new URL(`api/orders?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpGet<GetOrdersResponse>(url, {
            'Accept-Language': i18n.language,
        });
    }

    const create = (request: CreateOrderRequest) => {
        let url = `${import.meta.env.VITE_API_URL}api/orders`;
        return httpClient.httpPost<CreateOrderResponse>(url, request, {
            'Accept-Language': i18n.language,
        });
    }

    const update = (request: UpdateOrderRequest) => {
        let url = `${import.meta.env.VITE_API_URL}api/orders`;
        return httpClient.httpPut<UpdateOrderResponse>(url, request, {
            'Accept-Language': i18n.language,
        });
    }

    const submit = (request: SubmitOrderRequest) => {
        let url = `${import.meta.env.VITE_API_URL}api/orders/${request.id}/submit`;
        return httpClient.httpPost<SubmitOrderResponse>(url, request, {
            'Accept-Language': i18n.language,
        });
    }


    const state = useMemo(() => ({
        get,
        create,
        update,
        submit,
    }), [httpClient, i18n, i18n.language]);

    return state;
}