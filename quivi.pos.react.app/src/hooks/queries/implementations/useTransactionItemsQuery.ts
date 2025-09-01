import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import { PagedQueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";
import { TransactionItem } from "../../api/Dtos/transactionItems/TransactionItem";
import { GetTransactionItemsRequest } from "../../api/Dtos/transactionItems/GetTransactionItemsRequest";
import { useTransactionItemsApi } from "../../api/useTransactionItemsApi";

export const useTransactionItemsQuery = (request: GetTransactionItemsRequest | undefined) : PagedQueryResult<TransactionItem> => {
    const api = useTransactionItemsApi();
    
    const queryResult = useQueryable({
        queryName: "useTransactionItemsQuery",
        entityType: getEntityType(Entity.TransactionItems),
        request: request,
        getId: (e: TransactionItem) => e.id,
        query: request => api.get(request),
        refreshOnAnyUpdate: true,
    })

    const result = useMemo(() => ({
        isFirstLoading: queryResult.isFirstLoading,
        isLoading: queryResult.isLoading,
        data: queryResult.data,
        page: queryResult.response?.page ?? request?.page ?? 1,
        totalPages: queryResult.response?.totalPages ?? 1,
        totalItems: queryResult.response?.totalItems ?? 0,
    }), [queryResult, request?.page ?? 1])

    return result;
}