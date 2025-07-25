import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { useMemo } from "react";
import { GetTransactionsRequest } from "../../api/Dtos/transactions/GetTransactionsRequest";
import { Transaction } from "../../api/Dtos/transactions/Transaction";
import { useTransactionApi } from "../../api/useTransactionsApi";

export const useTransactionsQuery = (request: GetTransactionsRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = useTransactionApi();

    const queryResult = useQueryable({
        queryName: "useLocalsQuery",
        entityType: getEntityType(Entity.Transactions),
        request: user.merchantId == undefined || request == undefined ? undefined : {
            ...request,
            merchant: user.merchantId,
            subMerchantId: user.subMerchantId,
        },
        getIdsFilter: r => r.ids,
        getId: (e: Transaction) => e.id,
        query: request => api.get(request),

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