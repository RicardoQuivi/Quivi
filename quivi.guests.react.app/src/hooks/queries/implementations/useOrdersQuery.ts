import { useMemo } from "react";
import type { GetOrdersRequest } from "../../api/Dtos/orders/GetOrdersRequest";
import type { Order } from "../../api/Dtos/orders/Order";
import { useOrdersApi } from "../../api/useOrdersApi";
import { Entity, getEntityType } from "../../EntitiesName";
import type { PagedQueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";

export const useOrdersQuery = (request: GetOrdersRequest | undefined): PagedQueryResult<Order> => {
    const api = useOrdersApi();

    const query = useQueryable({
        queryName: "useOrdersQuery",
        entityType: getEntityType(Entity.Orders),
        getId: (item: Order) => item.id,
        request: request,
        query: request => api.get(request),
        getIdsFilter: (r) => r.ids,
        
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
        isFirstLoading: query.isFirstLoading,
        isLoading: query.isLoading,
        data: query.data,
        page: query.response?.page ?? 0,
        totalPages: query.response?.totalPages ?? 0,
        totalItems: query.response?.totalItems ?? 0,
    }), [query])
    
    return result;
}