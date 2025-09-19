import { useMemo } from "react";
import { GetOrdersRequest } from "../../api/Dtos/orders/GetOrdersRequest";
import { Order } from "../../api/Dtos/orders/Order";
import { Entity, getEntityType } from "../../EntitiesName";
import { PagedQueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";
import { useOrdersApi } from "../../api/useOrdersApi";

export const useOrdersQuery = (request: GetOrdersRequest | undefined) : PagedQueryResult<Order> => {
    const api = useOrdersApi();

    const queryResult = useQueryable({
        queryName: "useOrdersQuery",
        entityType: getEntityType(Entity.Orders),
        request: request == undefined ? undefined : {
            ...request,
        },
        getId: (e: Order) => e.id,
        query: api.get,

        refreshOnAnyUpdate: request?.ids == undefined,
        canUseOptimizedResponse: r => r.ids != undefined,
        getResponseFromEntities: (e) => ({
            data: e,
            page: 0,
            totalPages: 1,
            totalItems: 1,
        }),       
    })
    
    const result = useMemo(() => ({
        isFirstLoading: queryResult.isFirstLoading,
        isLoading: queryResult.isLoading,
        data: queryResult.data,
        page: queryResult.response?.page ?? request?.page ?? 1,
        totalPages: queryResult.response?.totalPages ?? 0,
        totalItems: queryResult.response?.totalItems ?? 0,
    }), [queryResult, request?.page ?? 1])
    
    return result;
}