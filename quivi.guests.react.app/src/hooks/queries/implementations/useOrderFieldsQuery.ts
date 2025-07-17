import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import type { PagedQueryResult } from "../QueryResult";
import type { OrderField } from "../../api/Dtos/orderFields/OrderField";
import { useQueryable } from "../useQueryable";

interface Request {
    readonly channelId: string;
    readonly languageIso: string;
}
export const useOrderFieldsQuery = (request: Request | undefined): PagedQueryResult<OrderField> => {

    const query = useQueryable({
        queryName: "useOrderFieldsQuery",
        entityType: getEntityType(Entity.Sessions),
        getId: (item: OrderField) => item.id,
        request: request,
        query: async _ => {
            return {
                data: [],
                page: 0,
                totalPages: 0,
                totalItems: 0,
            }
        },

        refreshOnAnyUpdate: true,
        canUseOptimizedResponse: _ => false,
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