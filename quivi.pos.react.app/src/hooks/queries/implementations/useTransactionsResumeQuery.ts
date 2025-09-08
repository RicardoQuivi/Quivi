import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import { QueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";
import { useTransactionsApi } from "../../api/useTransactionsApi";
import { GetTransactionsResumeRequest } from "../../api/Dtos/transactions/GetTransactionsResumeRequest";
import { TransactionResume } from "../../api/Dtos/transactions/TransactionResume";

export const useTransactionsResumeQuery = (request: GetTransactionsResumeRequest | undefined) : QueryResult<TransactionResume | undefined> => {
    const api = useTransactionsApi();
    
    const queryResult = useQueryable({
        queryName: "useTransactionsResumeQuery",
        entityType: getEntityType(Entity.TransactionResumes),
        request: request,
        getId: (_e: TransactionResume) => "resume",
        query: async r => {
            const response = await api.getResume(r);
            return {
                data: [response.data],
            }
        },

        refreshOnAnyUpdate: true, 
    })

    const result = useMemo(() => ({
        isFirstLoading: queryResult.isFirstLoading,
        isLoading: queryResult.isLoading,
        data: queryResult.data.length == 0 ? undefined : queryResult.data[0],
    }), [queryResult])
    
    return result;
}