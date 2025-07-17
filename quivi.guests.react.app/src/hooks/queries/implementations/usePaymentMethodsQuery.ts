import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import type { PagedQueryResult } from "../QueryResult";
import { usePaymentMethodsApi } from "../../api/usePaymentMethodsApi";
import type { GetPaymentMethodsRequest } from "../../api/Dtos/paymentmethods/GetPaymentMethodsRequest";
import type { PaymentMethod } from "../../api/Dtos/paymentmethods/PaymentMethod";

export const usePaymentMethodsQuery = (request: GetPaymentMethodsRequest | undefined): PagedQueryResult<PaymentMethod> => {
    const api = usePaymentMethodsApi();

    const query = useQueryable({
        queryName: "useTransactionsQuery",
        entityType: getEntityType(Entity.Transactions),
        getId: (item: PaymentMethod) => item.id,
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