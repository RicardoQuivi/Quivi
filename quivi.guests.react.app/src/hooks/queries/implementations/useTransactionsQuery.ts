import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import type { PagedQueryResult } from "../QueryResult";
import { useTransactionsApi } from "../../api/useTransactionsApi";
import type { GetTransactionsRequest } from "../../api/Dtos/transactions/GetTransactionsRequest";
import type { Transaction } from "../../api/Dtos/transactions/Transaction";

export const useTransactionsQuery = (request: GetTransactionsRequest | undefined): PagedQueryResult<Transaction> => {
    const api = useTransactionsApi();

    const query = useQueryable({
        queryName: "useTransactionsQuery",
        entityType: getEntityType(Entity.Transactions),
        getId: (item: Transaction) => item.id,
        request: request,
        query: request => api.get(request),

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