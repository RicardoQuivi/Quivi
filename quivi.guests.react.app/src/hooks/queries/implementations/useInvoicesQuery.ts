import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import type { Invoice } from "../../api/Dtos/invoices/Invoice";
import type { PagedQueryResult } from "../QueryResult";
import type { GetInvoicesRequest } from "../../api/Dtos/invoices/GetInvoicesRequest";

export const useInvoicesQuery = (request: GetInvoicesRequest | undefined): PagedQueryResult<Invoice> => {
    const query = useQueryable({
        queryName: "useInvoicesQuery",
        entityType: getEntityType(Entity.Invoices),
        getId: (item: Invoice) => item.id,
        request: request,
        query: async _ => ({
            data: [],
            page: 0,
            totalPages: 0,
            totalItems: 0,
        }),

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