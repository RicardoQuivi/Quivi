import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import { PagedQueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";
import { useTransactionsApi } from "../../api/useTransactionsApi";
import { GetTransactionsRequest } from "../../api/Dtos/transactions/GetTransactionsRequest";
import { Transaction } from "../../api/Dtos/transactions/Transaction";

export const useTransactionsQuery = (request: GetTransactionsRequest | undefined) : PagedQueryResult<Transaction> => {
    const api = useTransactionsApi();
    
    const queryResult = useQueryable({
        queryName: "useTransactionsQuery",
        entityType: getEntityType(Entity.Transactions),
        request: request,
        getId: (e: Transaction) => e.id,
        query: api.get,
        refreshOnAnyUpdate: true,

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
        page: queryResult.response?.page ?? 0,
        totalPages: queryResult.response?.totalPages ?? 0,
        totalItems: queryResult.response?.totalItems ?? 0,
    }), [queryResult])
    
    return result;
}