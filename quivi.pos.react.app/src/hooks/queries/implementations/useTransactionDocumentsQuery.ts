import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import { PagedQueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";
import { useTransactionDocumentsApi } from "../../api/useTransactionDocumentsApi";
import { GetTransactionDocumentsRequest } from "../../api/Dtos/transactionDocuments/GetTransactionDocumentsRequest";
import { TransactionDocument } from "../../api/Dtos/transactionDocuments/TransactionDocument";

export const useTransactionDocumentsQuery = (request: GetTransactionDocumentsRequest | undefined) : PagedQueryResult<TransactionDocument> => {
    const api = useTransactionDocumentsApi();
    
    const queryResult = useQueryable({
        queryName: "useTransactionDocumentsQuery",
        entityType: getEntityType(Entity.TransactionItems),
        request: request,
        getId: (e: TransactionDocument) => e.id,
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